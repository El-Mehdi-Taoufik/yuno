using Avalonia.Controls;
using Avalonia.Interactivity;
using YunoHub.Desktop.Models;
using YunoHub.Desktop.Services;

namespace YunoHub.Desktop;

public partial class MainWindow : Window
{
    private readonly ProfileStore _profiles = new();
    private TransformSettings _settings = new();

    public MainWindow()
    {
        InitializeComponent();
        _settings = _profiles.LoadCurrent() ?? new TransformSettings();
        ShowSettings(_settings);
        AngleSnapSlider.PropertyChanged += (_, _) => AngleSnapValue.Text = AngleSnapSlider.Value.ToString("0.00");
        AccelerationSlider.PropertyChanged += (_, _) => AccelerationValue.Text = AccelerationSlider.Value.ToString("0.00");
        BrakeStrengthSlider.PropertyChanged += (_, _) => BrakeStrengthValue.Text = BrakeStrengthSlider.Value.ToString("0.00");
        BrakeDelaySlider.PropertyChanged += (_, _) => BrakeDelayValue.Text = $"{BrakeDelaySlider.Value:0} ms";
    }

    private void ShowSettings(TransformSettings s)
    {
        RippleToggle.IsChecked = s.RippleControlEnabled;
        MotionSyncToggle.IsChecked = s.MotionSyncEnabled;
        AngleSnapSlider.Value = s.AngleSnapStrength;
        AccelerationSlider.Value = s.AccelIntensity;
        BrakeToggle.IsChecked = s.BrakeEnabled;
        BrakeStrengthSlider.Value = s.BrakeStrength;
        BrakeDelaySlider.Value = s.BrakeDelayMs;
        SensXInput.Value = (decimal)s.SensX;
        SensYInput.Value = (decimal)s.SensY;
        DpiXInput.Value = (decimal)s.DpiScaleX;
        DpiYInput.Value = (decimal)s.DpiScaleY;
    }

    private TransformSettings ReadSettings() => new()
    {
        RippleControlEnabled = RippleToggle.IsChecked ?? false,
        MotionSyncEnabled = MotionSyncToggle.IsChecked ?? false,
        AngleSnapStrength = AngleSnapSlider.Value,
        AccelIntensity = AccelerationSlider.Value,
        BrakeEnabled = BrakeToggle.IsChecked ?? false,
        BrakeStrength = BrakeStrengthSlider.Value,
        BrakeDelayMs = BrakeDelaySlider.Value,
        SensX = (double)(SensXInput.Value ?? 1),
        SensY = (double)(SensYInput.Value ?? 1),
        DpiScaleX = (double)(DpiXInput.Value ?? 1),
        DpiScaleY = (double)(DpiYInput.Value ?? 1)
    };

    private void Apply_Click(object? sender, RoutedEventArgs e)
    {
        _settings = ReadSettings();
        StatusText.Text = "Settings applied to local preview.";
        ProfileHint.Text = "Unsaved changes in this session";
    }

    private void SaveProfile_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _settings = ReadSettings();
            var name = string.IsNullOrWhiteSpace(ProfileName.Text) ? "Default" : ProfileName.Text;
            _profiles.Save(name, _settings);
            ProfileHint.Text = $"Saved profile: {name}";
            StatusText.Text = "Profile saved.";
        }
        catch (Exception ex) { StatusText.Text = ex.Message; }
    }

    private void LoadProfile_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var name = string.IsNullOrWhiteSpace(ProfileName.Text) ? "Default" : ProfileName.Text;
            var loaded = _profiles.Load(name);
            if (loaded is null) { StatusText.Text = "Profile not found."; return; }
            _settings = loaded;
            ShowSettings(_settings);
            ProfileHint.Text = $"Loaded profile: {name}";
            StatusText.Text = "Profile loaded.";
        }
        catch (Exception ex) { StatusText.Text = ex.Message; }
    }

    private void Reset_Click(object? sender, RoutedEventArgs e)
    {
        _settings = new TransformSettings();
        ShowSettings(_settings);
        StatusText.Text = "Defaults restored (not saved).";
    }
}
