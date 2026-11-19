using System.Text.Json;
using System.Text.Json.Serialization;
using YunoHub.Desktop.Models;

namespace YunoHub.Desktop.Services;

public sealed class ProfileStore
{
    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YunoHub", "Profiles");
    private readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ProfileStore() => Directory.CreateDirectory(_directory);

    public IReadOnlyList<string> ListProfiles() =>
        Directory.EnumerateFiles(_directory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public void Save(string name, TransformSettings settings)
    {
        var safe = Sanitize(name);
        File.WriteAllText(Path.Combine(_directory, safe + ".json"),
            JsonSerializer.Serialize(settings, _json));
        File.WriteAllText(Path.Combine(_directory, "current.txt"), safe);
    }

    public TransformSettings? Load(string name)
    {
        var path = Path.Combine(_directory, Sanitize(name) + ".json");
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize<TransformSettings>(File.ReadAllText(path), _json);
    }

    public void Export(string profileName, string destinationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        var settings = Load(profileName) ?? throw new FileNotFoundException("Profile not found.", profileName);
        File.WriteAllText(destinationPath, JsonSerializer.Serialize(settings, _json));
    }

    public void Import(string sourcePath, string profileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("Profile file not found.", sourcePath);
        var settings = JsonSerializer.Deserialize<TransformSettings>(File.ReadAllText(sourcePath), _json)
            ?? throw new InvalidDataException("The profile file is empty or invalid.");
        Validate(settings);
        Save(profileName, settings);
    }

    public bool Delete(string profileName)
    {
        var path = Path.Combine(_directory, Sanitize(profileName) + ".json");
        if (!File.Exists(path)) return false;
        File.Delete(path);
        var marker = Path.Combine(_directory, "current.txt");
        if (File.Exists(marker) && string.Equals(File.ReadAllText(marker).Trim(), Sanitize(profileName), StringComparison.OrdinalIgnoreCase))
            File.Delete(marker);
        return true;
    }

    private static void Validate(TransformSettings s)
    {
        if (!double.IsFinite(s.SensX) || s.SensX is < 0.1 or > 5 ||
            !double.IsFinite(s.SensY) || s.SensY is < 0.1 or > 5 ||
            !double.IsFinite(s.DpiScaleX) || s.DpiScaleX is < 0.1 or > 5 ||
            !double.IsFinite(s.DpiScaleY) || s.DpiScaleY is < 0.1 or > 5 ||
            !double.IsFinite(s.AngleSnapStrength) || s.AngleSnapStrength is < 0 or > 1 ||
            !double.IsFinite(s.AccelIntensity) || s.AccelIntensity is < -1 or > 1 ||
            !double.IsFinite(s.BrakeStrength) || s.BrakeStrength is < 0 or > 1 ||
            !double.IsFinite(s.BrakeDelayMs) || s.BrakeDelayMs is < 20 or > 600)
            throw new InvalidDataException("Profile values are outside the supported ranges.");
    }

    public TransformSettings? LoadCurrent()
    {
        var marker = Path.Combine(_directory, "current.txt");
        if (!File.Exists(marker)) return null;
        var name = File.ReadAllText(marker).Trim();
        return string.IsNullOrWhiteSpace(name) ? null : Load(name);
    }

    private static string Sanitize(string value)
    {
        var cleaned = new string((value ?? "").Trim()
            .Where(c => char.IsLetterOrDigit(c) || c is '-' or '_' or ' ')
            .ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(cleaned))
            throw new ArgumentException("Enter a valid profile name.");
        return cleaned;
    }
}
