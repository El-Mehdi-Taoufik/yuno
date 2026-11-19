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
