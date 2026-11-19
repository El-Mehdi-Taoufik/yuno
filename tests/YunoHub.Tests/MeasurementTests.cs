using Xunit;
using YunoHub.Desktop.Core;

namespace YunoHub.Tests;

public sealed class MeasurementTests
{
    [Fact]
    public void PollingRateTracker_ComputesInstantAndAverageRate()
    {
        var tracker = new PollingRateTracker();
        tracker.Record(1.000);
        tracker.Record(1.001);
        tracker.Record(1.002);

        Assert.InRange(tracker.InstantHz, 999, 1001);
        Assert.InRange(tracker.AverageHz, 999, 1001);
        Assert.Equal(1000, tracker.NominalHz);
    }

    [Fact]
    public void PollingRateTracker_IgnoresNonIncreasingTimestamps()
    {
        var tracker = new PollingRateTracker();
        tracker.Record(2.0);
        tracker.Record(2.0);
        tracker.Record(1.0);

        Assert.Equal(0, tracker.InstantHz);
    }

    [Fact]
    public void DpiMeasurementSession_CalculatesDpiAndSpread()
    {
        var session = new DpiMeasurementSession(distanceCm: 2.54, targetPasses: 3);
        session.AddPass(800);
        session.AddPass(810);
        session.AddPass(790);

        Assert.True(session.IsComplete);
        Assert.Equal(800, session.MeasuredDpi, 6);
        Assert.Equal(2.5, session.SpreadPercent, 6);
    }

    [Fact]
    public void DpiMeasurementSession_RejectsMorePassesThanConfigured()
    {
        var session = new DpiMeasurementSession(distanceCm: 5, targetPasses: 1);
        session.AddPass(500);

        Assert.Throws<InvalidOperationException>(() => session.AddPass(510));
    }
}
