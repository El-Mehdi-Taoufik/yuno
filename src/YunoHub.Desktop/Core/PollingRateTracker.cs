namespace YunoHub.Desktop.Core;

/// <summary>Estimates polling rate from externally supplied raw-input event timestamps.</summary>
public sealed class PollingRateTracker
{
    private static readonly double[] NominalRates = [125, 250, 500, 1000, 2000, 4000, 8000];
    private readonly Queue<double> _intervals = new();
    private double? _lastTimestamp;
    private double _average;
    private double _maximum;

    public double InstantHz { get; private set; }
    public double AverageHz => _average;
    public double MaxHz => _maximum;
    public double NominalHz => NominalRates.OrderBy(x => Math.Abs(x - AverageHz)).FirstOrDefault();

    public void Reset()
    {
        _intervals.Clear();
        _lastTimestamp = null;
        InstantHz = _average = _maximum = 0;
    }

    public void Record(double timestampSeconds)
    {
        if (!double.IsFinite(timestampSeconds)) return;
        if (_lastTimestamp is not null)
        {
            var interval = timestampSeconds - _lastTimestamp.Value;
            if (interval > 0 && interval <= 1)
            {
                _intervals.Enqueue(interval);
                while (_intervals.Count > 120) _intervals.Dequeue();
                InstantHz = 1.0 / interval;
                _maximum = Math.Max(_maximum, InstantHz);
                _average = 1.0 / _intervals.Average();
            }
        }
        _lastTimestamp = timestampSeconds;
    }
}
