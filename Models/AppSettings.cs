using System.Collections.Generic;

namespace WetheringWavesSteamHelper_WinUI.Models;

public class AppSettings
{
    // ===== 全局通用 Steam 配置（v2.2.0 起由设置页统一管理） =====
    public string SteamInstallPath { get; set; } = "";
    public string SteamLibraryPath { get; set; } = "";
    public string SteamId { get; set; } = "";

    // ===== 鸣潮专用 =====
    public string BuildId { get; set; } = "";
    public string Manifest { get; set; } = "";
    public string CnGameSource { get; set; } = "official";

    // ===== 自定义 Manifest（v2.2.0 新增；CustomManifestPresets 预留给 v2.3 多预设） =====
    public CustomManifestPreset CurrentCustomManifest { get; set; } = new();
    public List<CustomManifestPreset> CustomManifestPresets { get; set; } = new();

    // ===== 应用设置 =====
    public bool DeveloperMode { get; set; } = false;
    public bool DebugMode { get; set; } = false;
    public bool BetaChannel { get; set; } = false;
    public string Language { get; set; } = "zh-CN";
}

public class CustomManifestPreset
{
    public string Name { get; set; } = "默认";
    public string AppId { get; set; } = "";
    public string DepotId { get; set; } = "";
    public string BuildId { get; set; } = "";
    public string Manifest { get; set; } = "";
    public string GameDisplayName { get; set; } = "";
    public string InstallDir { get; set; } = "";
    public string ClientExePath { get; set; } = "";
    public string Language { get; set; } = "schinese";
}
