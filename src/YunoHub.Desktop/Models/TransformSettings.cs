namespace YunoHub.Desktop.Models;

public sealed class TransformSettings
{
    public double SensX { get; set; } = 1.0;
    public double SensY { get; set; } = 1.0;
    public double DpiScaleX { get; set; } = 1.0;
    public double DpiScaleY { get; set; } = 1.0;
    public double AngleSnapStrength { get; set; }
    public bool RippleControlEnabled { get; set; }
    public bool MotionSyncEnabled { get; set; }
    public double AccelIntensity { get; set; }
    public bool BrakeEnabled { get; set; } = true;
    public double BrakeStrength { get; set; } = 0.6;
    public double BrakeDelayMs { get; set; } = 200;

    public TransformSettings Clone() => (TransformSettings)MemberwiseClone();
}
