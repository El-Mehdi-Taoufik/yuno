using YunoHub.Desktop.Models;

namespace YunoHub.Desktop.Core;

/// <summary>
/// Pure managed transform. It processes supplied deltas only; no OS input hook is installed.
/// </summary>
public sealed class MouseTransform
{
    private double _carryX, _carryY, _snapX, _snapY, _rippleX, _rippleY;
    private double _syncX, _syncY, _lastFlush = -1, _syncInterval, _speedEma, _brakeStart = -1;

    public readonly record struct Output(int X, int Y, double AccelerationGain, double BrakeGain, bool BrakeActive);

    public void Reset()
    {
        _carryX = _carryY = _snapX = _snapY = _rippleX = _rippleY = 0;
        _syncX = _syncY = 0;
        _lastFlush = -1;
        _syncInterval = _speedEma = 0;
        _brakeStart = -1;
    }

    public Output Process(TransformSettings s, int dx, int dy, double nowSeconds, double dtSeconds, bool leftButtonDown)
    {
        ArgumentNullException.ThrowIfNull(s);
        var x = (double)dx;
        var y = (double)dy;

        if (s.RippleControlEnabled)
        {
            var magnitude2 = x * x + y * y;
            if (magnitude2 > 0 && magnitude2 < 2.56)
            {
                var alpha = dtSeconds <= 0 ? 0.5 : Math.Exp(-dtSeconds / 0.00144);
                _rippleX = _rippleX * alpha + x * (1 - alpha);
                _rippleY = _rippleY * alpha + y * (1 - alpha);
                x = _rippleX;
                y = _rippleY;
            }
            _rippleX = x;
            _rippleY = y;
        }

        var snap = Math.Clamp(s.AngleSnapStrength, 0, 1);
        if (snap > 0)
        {
            var k = dtSeconds <= 0 ? 0.8 : Math.Exp(-dtSeconds / 0.00448);
            _snapX = _snapX * k + x;
            _snapY = _snapY * k + y;
            var ax = Math.Abs(_snapX);
            var ay = Math.Abs(_snapY);
            var big = Math.Max(ax, ay);
            var small = Math.Min(ax, ay);
            if (big > 1)
            {
                var threshold = 0.14 + 0.36 * snap;
                var ratio = small / big;
                if (ratio < threshold)
                {
                    var damp = (0.6 + 0.39 * snap) * Math.Sqrt(1 - ratio / threshold);
                    if (ax >= ay) y *= 1 - damp;
                    else x *= 1 - damp;
                }
            }
        }

        var acceleration = 1.0;
        var intensity = Math.Clamp(s.AccelIntensity, -1, 1);
        if (intensity != 0 && dtSeconds > 0)
        {
            var speed = Math.Sqrt(x * x + y * y) / (dtSeconds * 1000);
            var ema = Math.Exp(-dtSeconds / 0.006);
            _speedEma = _speedEma * ema + speed * (1 - ema);
            acceleration = intensity > 0
                ? Math.Min(1 + 0.12 * intensity * _speedEma, 1 + 3 * intensity)
                : Math.Max(1 - 0.12 * -intensity * _speedEma, 1 - 0.85 * -intensity);
        }

        var brake = 1.0;
        var brakeActive = false;
        if (s.BrakeEnabled && leftButtonDown)
        {
            if (_brakeStart < 0) _brakeStart = nowSeconds;
            var delay = Math.Clamp(s.BrakeDelayMs / 1000.0, 0.02, 0.6);
            var phase = Math.Clamp((nowSeconds - _brakeStart) / delay, 0, 1);
            phase = phase * phase * (3 - 2 * phase);
            var strength = Math.Clamp(s.BrakeStrength, 0, 1);
            brake = 1 - 0.75 * strength * strength * phase;
            brakeActive = brake < 0.999;
        }
        else _brakeStart = -1;

        var outX = x * s.SensX * s.DpiScaleX * acceleration;
        var outY = y * s.SensY * s.DpiScaleY * acceleration * brake;

        if (s.MotionSyncEnabled)
        {
            if (dtSeconds > 0 && dtSeconds < 0.05)
                _syncInterval = _syncInterval > 0 ? _syncInterval * 0.9 + dtSeconds * 0.1 : dtSeconds;
            _syncX += outX;
            _syncY += outY;
            if (_lastFlush < 0) _lastFlush = nowSeconds;
            var interval = _syncInterval > 0 ? _syncInterval : dtSeconds;
            if (interval > 0 && nowSeconds - _lastFlush < interval * 0.9)
            {
                outX = 0;
                outY = 0;
            }
            else
            {
                outX = _syncX;
                outY = _syncY;
                _syncX = _syncY = 0;
                _lastFlush = nowSeconds;
            }
        }

        _carryX += outX;
        _carryY += outY;
        var ix = (int)_carryX;
        var iy = (int)_carryY;
        _carryX -= ix;
        _carryY -= iy;
        return new Output(ix, iy, acceleration, brake, brakeActive);
    }
}
