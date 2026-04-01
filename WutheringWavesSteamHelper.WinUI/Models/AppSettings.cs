using System.Text.Json;

namespace WutheringWavesSteamHelper.WinUI.Models;

public class AppSettings
{
    public string SteamLibraryPath { get; set; } = "";
    public string SteamInstallPath { get; set; } = "";
    public string SteamId { get; set; } = "";
    public string BuildId { get; set; } = "";
    public string Manifest { get; set; } = "";
    public string CnGameSource { get; set; } = "official";

    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "WutheringWavesSteamHelper");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    public static AppSettings Load()
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

    public bool Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
