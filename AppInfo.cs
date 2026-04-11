namespace WetheringWavesSteamHelper_WinUI;

/// <summary>
/// 应用版本信息的单一真相来源。
/// 修改此处即可同步更新所有界面中显示的版本文字。
/// </summary>
public static class AppInfo
{
    /// <summary>版本号，如 v2.0.0</summary>
    public const string Version = "v2.0.1";

    /// <summary>发布渠道/阶段，如 Alpha 1 Test、Beta、Release</summary>
    public const string Channel = "Release";

    /// <summary>完整版本字符串，用于界面显示，如 "v2.0.0 (Alpha 1 Test)"</summary>
    public const string FullVersion = $"{Version} ({Channel})";

    /// <summary>窗口标题</summary>
    public const string WindowTitle = $"鸣潮 Steam 助手 {FullVersion}";

    /// <summary>应用名称</summary>
    public const string AppName = "鸣潮 Steam 助手";

    /// <summary>版权信息</summary>
    public const string Copyright = "© 2026 KAMITSUBAKI METAVERSE R&D DIV";
}
