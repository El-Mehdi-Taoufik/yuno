using YunoHub.Desktop.Core;
using YunoHub.Desktop.Models;

namespace YunoHub.Tests;

public sealed class MouseTransformTests
{
    [Fact]
    public void IdentitySettings_PreserveIntegerMovement()
    {
        var transform = new MouseTransform();
        var output = transform.Process(new TransformSettings(), 12, -7, 1.0, 0.01, false);

        Assert.Equal(12, output.X);
        Assert.Equal(-7, output.Y);
        Assert.Equal(1.0, output.AccelerationGain, 6);
    }

    [Fact]
    public void Sensitivity_PreservesFractionalCountsAcrossCalls()
    {
        var transform = new MouseTransform();
        var settings = new TransformSettings { SensX = 0.5 };

        var first = transform.Process(settings, 1, 0, 1.0, 0, false);
        var second = transform.Process(settings, 1, 0, 1.01, 0, false);

        Assert.Equal(0, first.X);
        Assert.Equal(1, second.X);
    }

    [Fact]
    public void Brake_ReducesVerticalMovementAfterConfiguredDelay()
    {
        var transform = new MouseTransform();
        var settings = new TransformSettings
        {
            BrakeEnabled = true,
            BrakeStrength = 0.6,
            BrakeDelayMs = 200
        };

        transform.Process(settings, 0, 10, 1.0, 0.01, true);
        var output = transform.Process(settings, 0, 10, 1.2, 0.01, true);

        Assert.True(output.BrakeActive);
        Assert.InRange(output.BrakeGain, 0.72, 0.74);
        Assert.Equal(7, output.Y);
    }

    [Fact]
    public void Reset_ClearsFractionalCarry()
    {
        var transform = new MouseTransform();
        var settings = new TransformSettings { SensX = 0.5 };

        transform.Process(settings, 1, 0, 1.0, 0, false);
        transform.Reset();
        var afterReset = transform.Process(settings, 1, 0, 1.01, 0, false);

        Assert.Equal(0, afterReset.X);
    }
}
