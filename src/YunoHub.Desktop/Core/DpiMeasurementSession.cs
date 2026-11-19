namespace YunoHub.Desktop.Core;

/// <summary>Converts measured horizontal counts over a known physical distance into DPI.</summary>
public sealed class DpiMeasurementSession
{
    private readonly List<double> _passes = new();

    public DpiMeasurementSession(double distanceCm, int targetPasses = 3)
    {
        if (!double.IsFinite(distanceCm) || distanceCm <= 0)
            throw new ArgumentOutOfRangeException(nameof(distanceCm));
        if (targetPasses < 1 || targetPasses > 20)
            throw new ArgumentOutOfRangeException(nameof(targetPasses));
        DistanceCm = distanceCm;
        TargetPasses = targetPasses;
    }

    public double DistanceCm { get; }
    public int TargetPasses { get; }
    public int PassesDone => _passes.Count;
    public bool IsComplete => PassesDone >= TargetPasses;
    public IReadOnlyList<double> PassCounts => _passes.AsReadOnly();
    public double AverageCounts => _passes.Count == 0 ? 0 : _passes.Average();
    public double MeasuredDpi => AverageCounts / DistanceCm * 2.54;
    public double SpreadPercent => _passes.Count < 2 || AverageCounts <= 0
        ? 0
        : (_passes.Max() - _passes.Min()) / AverageCounts * 100.0;

    public void AddPass(double absoluteHorizontalCounts)
    {
        if (IsComplete) throw new InvalidOperationException("All measurement passes are already complete.");
        if (!double.IsFinite(absoluteHorizontalCounts) || absoluteHorizontalCounts <= 0)
            throw new ArgumentOutOfRangeException(nameof(absoluteHorizontalCounts));
        _passes.Add(absoluteHorizontalCounts);
    }

    public void Reset() => _passes.Clear();
}
