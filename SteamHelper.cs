using Microsoft.Win32;
using System.Text.Json;

namespace WutheringWavesSteamHelper
{
    public static class SteamHelper
    {
        private const string SteamRegistryKey = @"SOFTWARE\WOW6432Node\Valve\Steam";
        private const string SteamRegistryKey32 = @"SOFTWARE\Valve\Steam";
        private const int AppId = 3513350;

        public static string? DetectSteamInstallPath()
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
            ];

            foreach (var p in commonPaths)
            {
                if (Directory.Exists(p) && File.Exists(Path.Combine(p, "steam.exe")))
                    return p;
            }

            return null;
        }

        public static List<string> DetectSteamLibraryPaths()
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

        public static async Task<(string buildId, string manifest)?> FetchSteamDbInfoAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "WutheringWavesSteamHelper/1.0");

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
                            && depots2.TryGetProperty("3513351", out var depot)
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

        public static string GenerateAcfContent(string launcherPath, string buildId, string lastOwner, string manifest)
        {
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
		""3513351""
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
}
