using System.Text.Json.Serialization;

namespace WetheringWavesSteamHelper_WinUI.Models;

/// <summary>
/// 远程 version.json 的反序列化模型。
/// </summary>
public sealed class VersionInfo
{
    /// <summary>版本号字符串，如 "2.0.1"（不含 v 前缀）</summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = "";

    /// <summary>发布日期，如 "2026-04-11"</summary>
    [JsonPropertyName("releaseDate")]
    public string ReleaseDate { get; init; } = "";

    /// <summary>更新说明，在通知卡片中展示</summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = "";

    /// <summary>下载地址，按网络环境选择</summary>
    [JsonPropertyName("downloadUrl")]
    public DownloadUrls DownloadUrl { get; init; } = new();

    /// <summary>是否强制更新（锁定 UI 直到用户前往下载）</summary>
    [JsonPropertyName("forceUpdate")]
    public bool ForceUpdate { get; init; }
}

/// <summary>
/// 按地区区分的下载链接。
/// </summary>
public sealed class DownloadUrls
{
    /// <summary>国内下载地址（如百度网盘）</summary>
    [JsonPropertyName("domestic")]
    public string Domestic { get; init; } = "";

    /// <summary>境外下载地址（如 GitHub Releases）</summary>
    [JsonPropertyName("global")]
    public string Global { get; init; } = "";
}
