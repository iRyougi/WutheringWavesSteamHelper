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

    // ===== 自定义 Manifest（v2.2.0 新增；v2.3.0 起 CustomManifestPresets 升为唯一数据源） =====
    public CustomManifestPreset CurrentCustomManifest { get; set; } = new(); // 镜像选中预设，保向后兼容
    public List<CustomManifestPreset> CustomManifestPresets { get; set; } = new();
    public string CurrentCustomManifestName { get; set; } = ""; // v2.3.0 新增：记住上次选中的预设名

    // ===== 应用设置 =====
    public bool DeveloperMode { get; set; } = false;
    public bool DebugMode { get; set; } = false;
    public bool BetaChannel { get; set; } = false;
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 幂等地确保 <see cref="CustomManifestPresets"/> 至少有一个预设（v2.3.0 迁移逻辑）。
    /// 旧 v2.2.0 settings.json 无预设列表时：若 CurrentCustomManifest 含任一非默认字段，
    /// 则把它纳入列表；否则补一个空白「默认」预设。
    /// </summary>
    public void EnsureCustomManifestPresets()
    {
        if (CustomManifestPresets.Count > 0)
            return;

        var current = CurrentCustomManifest;
        var hasData = !string.IsNullOrWhiteSpace(current.AppId)
                   || !string.IsNullOrWhiteSpace(current.DepotId)
                   || !string.IsNullOrWhiteSpace(current.BuildId)
                   || !string.IsNullOrWhiteSpace(current.Manifest)
                   || !string.IsNullOrWhiteSpace(current.GameDisplayName)
                   || !string.IsNullOrWhiteSpace(current.InstallDir)
                   || !string.IsNullOrWhiteSpace(current.ClientExePath);

        if (hasData)
        {
            // 复制一份纳入列表，避免与 CurrentCustomManifest 共享同一引用（别名隐患）
            CustomManifestPresets.Add(new CustomManifestPreset
            {
                Name = string.IsNullOrWhiteSpace(current.Name) ? "默认" : current.Name,
                AppId = current.AppId,
                DepotId = current.DepotId,
                BuildId = current.BuildId,
                Manifest = current.Manifest,
                GameDisplayName = current.GameDisplayName,
                InstallDir = current.InstallDir,
                ClientExePath = current.ClientExePath,
                ExecutableFileName = current.ExecutableFileName,
                Language = current.Language,
            });
        }
        else
        {
            CustomManifestPresets.Add(new CustomManifestPreset { Name = "默认" });
        }
    }
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
    public string ClientExePath { get; set; } = "";        // 真实游戏 exe 完整路径（用于复制启动命令）
    public string ExecutableFileName { get; set; } = "";   // v2.3.0 新增：Steam 占位 exe 文件名
    public string Language { get; set; } = "schinese";
}
