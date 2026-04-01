using System.Text.Json;
using WetheringWavesSteamHelper_WinUI.Models;

namespace WetheringWavesSteamHelper_WinUI.Services;

public class SettingsService
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "WutheringWavesSteamHelper");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public bool Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
