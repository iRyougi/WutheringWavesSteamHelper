namespace WetheringWavesSteamHelper_WinUI.Models;

public class AppSettings
{
    // 基础配置
    public string SteamLibraryPath { get; set; } = "";
    public string SteamInstallPath { get; set; } = "";
    public string SteamId { get; set; } = "";
    public string BuildId { get; set; } = "";
    public string Manifest { get; set; } = "";
    public string CnGameSource { get; set; } = "official";

    // 应用设置
    public bool DeveloperMode { get; set; } = false;
    public bool DebugMode { get; set; } = false;
    public bool BetaChannel { get; set; } = false;
    public string Language { get; set; } = "zh-CN";
}
