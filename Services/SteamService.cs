using Microsoft.Win32;
using System.Text.Json;

namespace WetheringWavesSteamHelper_WinUI.Services;

public class SteamService
{
    private const string SteamRegistryKey = @"SOFTWARE\WOW6432Node\Valve\Steam";
    private const string SteamRegistryKey32 = @"SOFTWARE\Valve\Steam";
    private const int AppId = 3513350;
    private const int DepotId = 3513351;

    public string? DetectSteamInstallPath()
    {
        string? path = null;
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(SteamRegistryKey)
                         ?? Registry.LocalMachine.OpenSubKey(SteamRegistryKey32);
            path = key?.GetValue("InstallPath") as string;
        }
        catch { }

        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            return path;

        string[] commonPaths =
        [
            @"C:\Program Files (x86)\Steam",
            @"C:\Program Files\Steam",
            @"D:\Steam",
            @"D:\Program Files (x86)\Steam",
            @"D:\Program Files\Steam",
            @"E:\Steam",
            @"E:\Program Files (x86)\Steam",
        ];

        foreach (var p in commonPaths)
        {
            if (Directory.Exists(p) && File.Exists(Path.Combine(p, "steam.exe")))
                return p;
        }

        return null;
    }

    public List<string> DetectSteamLibraryPaths()
    {
        var paths = new List<string>();

        var steamPath = DetectSteamInstallPath();
        if (steamPath == null) return paths;

        var defaultLibrary = Path.Combine(steamPath, "steamapps");
        if (Directory.Exists(defaultLibrary))
            paths.Add(steamPath);

        var libraryFoldersVdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libraryFoldersVdf)) return paths;

        try
        {
            var content = File.ReadAllText(libraryFoldersVdf);
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("\"path\""))
                {
                    var parts = trimmed.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        // VDF 中路径用 \\ 转义，需要还原为单个 \
                        var libPath = parts[^1].Trim('"').Replace("\\\\", "\\");
                        if (Directory.Exists(libPath) && !paths.Contains(libPath, StringComparer.OrdinalIgnoreCase))
                            paths.Add(libPath);
                    }
                }
            }
        }
        catch { }

        return paths;
    }

    public async Task<(string buildId, string manifest)?> FetchSteamDbInfoAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "WutheringWavesSteamHelper/2.0");

            var url = $"https://api.steamcmd.net/v1/info/{AppId}";
            var response = await httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (root.TryGetProperty("status", out var status) && status.GetString() == "success"
                && root.TryGetProperty("data", out var data))
            {
                string? buildId = null;
                string? manifest = null;

                if (data.TryGetProperty(AppId.ToString(), out var appData))
                {
                    if (appData.TryGetProperty("depots", out var depots)
                        && depots.TryGetProperty("branches", out var branches)
                        && branches.TryGetProperty("public", out var publicBranch)
                        && publicBranch.TryGetProperty("buildid", out var buildIdElem))
                    {
                        buildId = buildIdElem.GetString();
                    }

                    if (appData.TryGetProperty("depots", out var depots2)
                        && depots2.TryGetProperty(DepotId.ToString(), out var depot)
                        && depot.TryGetProperty("manifests", out var manifests)
                        && manifests.TryGetProperty("public", out var publicManifest))
                    {
                        if (publicManifest.ValueKind == JsonValueKind.String)
                        {
                            manifest = publicManifest.GetString();
                        }
                        else if (publicManifest.TryGetProperty("gid", out var gid))
                        {
                            manifest = gid.GetString();
                        }
                    }

                    if (!string.IsNullOrEmpty(buildId) && !string.IsNullOrEmpty(manifest))
                        return (buildId, manifest);
                }
            }
        }
        catch { }

        return null;
    }

    public string? DetectWeGameInstallPath()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Rail\WutheringWaves");
            var path = key?.GetValue("InstallPath") as string;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                return path;
        }
        catch { }
        return null;
    }

    public List<string> DetectCnWutheringWavesPaths()
    {
        var results = new List<string>();
        var officialRelativePath = Path.Combine("Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");

        // 官方启动器注册表检测
        string[] registryKeys =
        [
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\KRInstall Wuthering Waves",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\KRInstall Wuthering Waves",
        ];

        foreach (var regKey in registryKeys)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(regKey);
                var installLocation = key?.GetValue("InstallPath") as string
                                   ?? key?.GetValue("InstallLocation") as string;
                if (!string.IsNullOrEmpty(installLocation))
                {
                    var exePath = Path.Combine(installLocation, officialRelativePath);
                    if (File.Exists(exePath) && !results.Contains(installLocation, StringComparer.OrdinalIgnoreCase))
                        results.Add(installLocation);
                }
            }
            catch { }
        }

        // WeGame 注册表检测 (HKCU\SOFTWARE\Rail\WutheringWaves)
        try
        {
            using var wegameKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Rail\WutheringWaves");
            var wegameInstall = wegameKey?.GetValue("InstallPath") as string;
            if (!string.IsNullOrEmpty(wegameInstall))
            {
                // WeGame 结构：{InstallPath}\Client\Binaries\Win64\Client-Win64-Shipping.exe
                var wegameExe = Path.Combine(wegameInstall, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
                if (File.Exists(wegameExe) && !results.Contains(wegameInstall, StringComparer.OrdinalIgnoreCase))
                    results.Add(wegameInstall);
            }
        }
        catch { }

        // 常见安装路径扫描
        var drives = DriveInfo.GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
            .Select(d => d.RootDirectory.FullName);

        string[] commonFolders =
        [
            @"Wuthering Waves",
            @"鸣潮",
            @"Kuro\Wuthering Waves",
            @"KuroGames\Wuthering Waves",
            @"Program Files\Wuthering Waves",
            @"Program Files (x86)\Wuthering Waves",
            @"Games\Wuthering Waves",
        ];

        foreach (var drive in drives)
        {
            foreach (var folder in commonFolders)
            {
                var candidatePath = Path.Combine(drive, folder);
                if (!Directory.Exists(candidatePath)) continue;

                // 检测官方启动器结构
                var officialExe = Path.Combine(candidatePath, officialRelativePath);
                if (File.Exists(officialExe) && !results.Contains(candidatePath, StringComparer.OrdinalIgnoreCase))
                    results.Add(candidatePath);

                // 检测 WeGame 结构（直接在安装目录下有 Client 文件夹）
                var wegameExe = Path.Combine(candidatePath, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
                if (File.Exists(wegameExe) && !results.Contains(candidatePath, StringComparer.OrdinalIgnoreCase))
                    results.Add(candidatePath);
            }
        }

        return results;
    }

    /// <summary>
    /// 生成 Steam 启动命令。
    /// installPath：官方启动器版本为安装根目录（含 Wuthering Waves Game 子目录），
    ///              WeGame 版本为含 Client 文件夹的安装根目录。
    /// </summary>
    public string GenerateLaunchCommand(string installPath)
    {
        // 优先检测 WeGame 结构（无 "Wuthering Waves Game" 层）
        var wegameExe = Path.Combine(installPath, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
        if (File.Exists(wegameExe))
            return $"\"{wegameExe}\" %command%";

        // 官方启动器结构
        var officialExe = Path.Combine(installPath, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
        return $"\"{officialExe}\" %command%";
    }

    /// <summary>
    /// 生成 ACF 文件内容。
    /// launcherPath 应为 Steam 安装目录中的 steam.exe 完整路径。
    /// </summary>
    public string GenerateAcfContent(string launcherPath, string buildId, string lastOwner, string manifest)
    {
        // ACF 中路径需要用双反斜杠转义
        var escapedLauncherPath = launcherPath.Replace("\\", "\\\\");

        return $@"""AppState""
{{
	""appid""		""3513350""
	""universe""		""1""
	""LauncherPath""		""{escapedLauncherPath}""
	""name""		""Wuthering Waves""
	""StateFlags""		""4""
	""installdir""		""Wuthering Waves""
	""LastUpdated""		""0""
	""LastPlayed""		""0""
	""SizeOnDisk""		""0""
	""StagingSize""		""0""
	""buildid""		""{buildId}""
	""LastOwner""		""{lastOwner}""
	""DownloadType""		""1""
	""UpdateResult""		""0""
	""BytesToDownload""		""0""
	""BytesDownloaded""		""0""
	""BytesToStage""		""0""
	""BytesStaged""		""0""
	""TargetBuildID""		""0""
	""AutoUpdateBehavior""		""1""
	""AllowOtherDownloadsWhileRunning""		""0""
	""ScheduledAutoUpdate""		""0""
	""InstalledDepots""
	{{
		""{DepotId}""
		{{
			""manifest""		""{manifest}""
			""size""		""0""
		}}
	}}
	""SharedDepots""
	{{
		""228989""		""228980""
	}}
	""UserConfig""
	{{
		""language""		""schinese""
	}}
	""MountedConfig""
	{{
		""language""		""schinese""
	}}
}}
";
    }
}
