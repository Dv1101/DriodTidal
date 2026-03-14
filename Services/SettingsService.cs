using System.Text.Json;

namespace DroidTidal.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private Dictionary<string, object> _settings = new();

    private const string ApiBaseUrlKey = "ApiBaseUrl";
    private const string AudioQualityKey = "AudioQuality";
    private const string VolumeKey = "Volume";

    private const string DefaultApiBaseUrl = "https://hifi.520.be";

    public SettingsService()
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DroidTidal", "settings.json");
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
            }
        }
        catch
        {
            _settings = new();
        }
    }

    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            var json = JsonSerializer.Serialize(_settings);
            File.WriteAllText(_settingsPath, json);
        }
        catch { }
    }

    public string ApiBaseUrl
    {
        get => _settings.TryGetValue(ApiBaseUrlKey, out var val) ? val.ToString() ?? DefaultApiBaseUrl : DefaultApiBaseUrl;
        set { _settings[ApiBaseUrlKey] = value; SaveSettings(); }
    }

    public string AudioQuality
    {
        get => _settings.TryGetValue(AudioQualityKey, out var val) ? val.ToString() ?? "HI_RES_LOSSLESS" : "HI_RES_LOSSLESS";
        set { _settings[AudioQualityKey] = value; SaveSettings(); }
    }

    public double Volume
    {
        get
        {
            if (_settings.TryGetValue(VolumeKey, out var val) && val is JsonElement el && el.TryGetDouble(out var d))
                return d;
            return 100.0;
        }
        set { _settings[VolumeKey] = value; SaveSettings(); }
    }
}
