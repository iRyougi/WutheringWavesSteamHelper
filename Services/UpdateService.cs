using System.Net.Http;
using System.Text.Json;
using WetheringWavesSteamHelper_WinUI.Models;

namespace WetheringWavesSteamHelper_WinUI.Services;

/// <summary>
/// 更新检查服务（单例）。
/// 负责检测 GitHub 连通性、拉取 version.json、比较版本号并触发更新通知事件。
/// 所有网络错误均静默吞噬，不影响应用正常启动。
/// Debug 模式开启时，从本地 127.0.0.1:9090 拉取（方便测试）。
/// </summary>
public sealed class UpdateService
{
    // ── 单例 ─────────────────────────────────────────────────────────────────
    public static UpdateService Instance { get; } = new();
    private UpdateService() { }

    // ── 生产地址 ──────────────────────────────────────────────────────────────
    private const string GitHubRawUrl =
        "https://raw.githubusercontent.com/iRyougi/WutheringWavesSteamHelper/master/version.json";

    private const string MirrorRawUrl =
        "https://ghfast.top/https://raw.githubusercontent.com/iRyougi/WutheringWavesSteamHelper/master/version.json";

    private const string GitHubProbeUrl =
        "https://raw.githubusercontent.com/iRyougi/WutheringWavesSteamHelper/master/version.json";

    // ── 测试版渠道地址 ────────────────────────────────────────────────────────
    private const string BetaRawUrl =
        "https://raw.githubusercontent.com/iRyougi/WutheringWavesSteamHelper/preview/version.json";

    private const string BetaMirrorUrl =
        "https://ghfast.top/https://raw.githubusercontent.com/iRyougi/WutheringWavesSteamHelper/preview/version.json";

    // ── Debug 模式本地地址 ────────────────────────────────────────────────────
    private const string DebugLocalUrl = "http://127.0.0.1:9090/version.json";

    // ── 事件 ──────────────────────────────────────────────────────────────────
    /// <summary>
    /// 当发现新版本时触发。参数：(message, downloadUrl, forceUpdate)
    /// </summary>
    public event Action<string, string, bool>? UpdateAvailable;

    // ── 缓存上次检查结果（页面晚于检查完成时加载，可直接回放） ─────────────────
    private string? _cachedMessage;
    private string? _cachedDownloadUrl;
    private bool    _cachedForceUpdate;
    public  bool    HasPendingUpdate { get; private set; }

    // ── HTTP 客户端（复用） ────────────────────────────────────────────────────
    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = true
    });

    // ── JSON 反序列化选项 ─────────────────────────────────────────────────────
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ── 设置读取（每次实时读，确保与开关同步） ───────────────────────────────────
    private static AppSettings LoadSettings() => new SettingsService().Load();
    private static bool IsDebugMode  => LoadSettings().DebugMode;
    private static bool IsBetaChannel => LoadSettings().BetaChannel;

    // ─────────────────────────────────────────────────────────────────────────
    // 公共 API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 异步检查更新。
    /// Debug 模式：直接请求 127.0.0.1:9090，跳过 GitHub 探测。
    /// 生产模式：探测 GitHub 连通性，失败则切换镜像站。
    /// </summary>
    public async Task CheckUpdateAsync()
    {
        try
        {
            bool debug = IsDebugMode;
            bool beta  = IsBetaChannel;

            // URL 优先级：Debug > Beta > 生产（GitHub / 镜像）
            var versionJsonUrl = debug
                ? DebugLocalUrl
                : await GetVersionJsonUrlAsync(beta).ConfigureAwait(false);

            var json = await FetchJsonAsync(versionJsonUrl).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json)) return;

            var info = JsonSerializer.Deserialize<VersionInfo>(json, _jsonOptions);
            if (info is null) return;

            // 解析版本号：格式为 "2.0.2" 或 "2.0.2(Beta 1)" 或 "v2.0.2(Beta 1)"
            // 本地版本来自 AppInfo.FullVersion，格式为 "v2.0.2 (Beta)"
            var remoteVersionFull = info.Version.TrimStart('v');
            var localVersionFull  = AppInfo.FullVersion.TrimStart('v');

            // 提取主版本号（括号前的部分）
            var remoteVersionStr = ExtractMainVersion(remoteVersionFull);
            var localVersionStr  = ExtractMainVersion(localVersionFull);

            if (!System.Version.TryParse(remoteVersionStr, out var remoteVersion)) return;
            if (!System.Version.TryParse(localVersionStr,  out var localVersion))  return;

            // 主版本号不同时，直接用主版本号决定
            if (remoteVersion != localVersion)
            {
                if (remoteVersion <= localVersion) return;
            }
            else if (beta)
            {
                // 主版本号相同时，若为 Beta 渠道，比较括号内的子版本（如 Beta 1、Beta 2）
                var remoteSubVersion = ExtractSubVersion(remoteVersionFull);
                var localSubVersion  = ExtractSubVersion(localVersionFull);

                // 无法解析子版本，或远端子版本不大于本地，则跳过
                if (remoteSubVersion <= localSubVersion) return;
            }
            else
            {
                // 正式渠道：主版本相同即为最新
                return;
            }

            var downloadUrl = debug
                ? (string.IsNullOrWhiteSpace(info.DownloadUrl.Global)
                    ? info.DownloadUrl.Domestic
                    : info.DownloadUrl.Global)
                : await GetDownloadUrlAsync(info).ConfigureAwait(false);

            _cachedMessage     = info.Message;
            _cachedDownloadUrl = downloadUrl;
            _cachedForceUpdate = info.ForceUpdate;
            HasPendingUpdate   = true;

            UpdateAvailable?.Invoke(info.Message, downloadUrl, info.ForceUpdate);
        }
        catch
        {
            // 网络异常等均静默忽略
        }
    }

    /// <summary>
    /// 若已有缓存的更新结果，立即向新订阅者回放一次事件。
    /// 在 SettingsPage.OnLoaded 订阅后调用，解决"检查比页面加载早"的时序问题。
    /// </summary>
    public void ReplayIfPending()
    {
        if (HasPendingUpdate && _cachedMessage is not null && _cachedDownloadUrl is not null)
        {
            UpdateAvailable?.Invoke(_cachedMessage, _cachedDownloadUrl, _cachedForceUpdate);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 私有辅助
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 判断 GitHub 直连是否可用（3 秒超时）。
    /// 可用则返回直链，否则返回镜像站链接。
    /// </summary>
    private static async Task<string> GetVersionJsonUrlAsync(bool beta = false)
    {
        var probeUrl  = beta ? BetaRawUrl    : GitHubProbeUrl;
        var directUrl = beta ? BetaRawUrl    : GitHubRawUrl;
        var mirrorUrl = beta ? BetaMirrorUrl : MirrorRawUrl;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var resp = await _httpClient.GetAsync(probeUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                                              .ConfigureAwait(false);
            if (resp.IsSuccessStatusCode) return directUrl;
        }
        catch
        {
            // 超时或连接失败 → 使用镜像站
        }

        return mirrorUrl;
    }

    /// <summary>
    /// 根据网络环境，从 version.json 中选择合适的下载地址。
    /// GitHub 直连可用 → 返回 Global；否则返回 Domestic。
    /// </summary>
    private static async Task<string> GetDownloadUrlAsync(VersionInfo info)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var resp = await _httpClient.GetAsync(GitHubProbeUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                                              .ConfigureAwait(false);
            if (resp.IsSuccessStatusCode) return info.DownloadUrl.Global;
        }
        catch
        {
            // 超时 → 国内镜像
        }

        return string.IsNullOrWhiteSpace(info.DownloadUrl.Domestic)
            ? info.DownloadUrl.Global
            : info.DownloadUrl.Domestic;
    }

    /// <summary>
    /// 从指定 URL 获取文本内容，失败返回 null。
    /// </summary>
    private static async Task<string?> FetchJsonAsync(string url)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            return await _httpClient.GetStringAsync(url, cts.Token).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从版本字符串中提取主版本号（括号前的部分，去除空格）。
    /// 例："2.0.2(Beta 1)" → "2.0.2"，"2.0.2 (Release)" → "2.0.2"
    /// </summary>
    private static string ExtractMainVersion(string versionFull)
    {
        var idx = versionFull.IndexOf('(');
        return (idx >= 0 ? versionFull[..idx] : versionFull).Trim();
    }

    /// <summary>
    /// 从版本字符串中提取括号内的子版本序号（仅限 Beta/Alpha 等含数字的情况）。
    /// 例："2.0.2 (Beta 1)" → 1，"2.0.2 (Beta 2)" → 2，"2.0.2 (Release)" → 0（忽略）
    /// 无法解析时返回 0。
    /// </summary>
    private static int ExtractSubVersion(string versionFull)
    {
        var start = versionFull.IndexOf('(');
        var end   = versionFull.IndexOf(')');
        if (start < 0 || end < 0 || end <= start) return 0;

        var inner = versionFull[(start + 1)..end].Trim(); // e.g. "Beta 1"

        // 取括号内最后一个数字段
        var parts = inner.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts.Reverse())
        {
            if (int.TryParse(part, out var num)) return num;
        }

        return 0;
    }
}
