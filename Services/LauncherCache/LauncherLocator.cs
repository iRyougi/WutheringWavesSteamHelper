using System.Security;
using Microsoft.Win32;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>汇总可能的新版启动器目录（含 kr_game_cache\kr_game_temp.bin 的目录）。</summary>
public static class LauncherLocator
{
    private const string KuroKeyPath = @"Software\kurogame";
    private const int MaxRegistryDepth = 6;

    /// <summary>
    /// 候选顺序：savedDir → 注册表 kurogame 下所有字符串值 → extraInstallRoots；去重后只保留有效启动器目录。
    /// </summary>
    public static IReadOnlyList<string> FindCandidates(string? savedDir, IEnumerable<string> extraInstallRoots)
    {
        var raw = new List<string?> { savedDir };
        raw.AddRange(ScanRegistryStrings());
        raw.AddRange(extraInstallRoots);
        return FilterCandidates(raw);
    }

    /// <summary>把任意字符串规整为候选目录（文件取所在目录），去重并过滤无效项。</summary>
    public static IReadOnlyList<string> FilterCandidates(IEnumerable<string?> values)
    {
        var result = new List<string>();
        foreach (var value in values)
        {
            var dir = Normalize(value);
            if (dir is null || !LauncherCacheFiles.IsLauncherDir(dir)) continue;
            if (!result.Contains(dir, StringComparer.OrdinalIgnoreCase))
                result.Add(dir);
        }
        return result;
    }

    private static string? Normalize(string? value)
    {
        var v = value?.Trim().Trim('"').Trim();
        if (string.IsNullOrEmpty(v)) return null;
        try
        {
            if (!Path.IsPathFullyQualified(v)) return null;
            if (Directory.Exists(v)) return Path.TrimEndingDirectorySeparator(Path.GetFullPath(v));
            if (File.Exists(v)) return Path.GetDirectoryName(Path.GetFullPath(v));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
        return null;
    }

    private static IEnumerable<string> ScanRegistryStrings()
    {
        var results = new List<string>();
        var roots = new List<(RegistryHive Hive, RegistryView View)>
        {
            (RegistryHive.CurrentUser, RegistryView.Default),
            (RegistryHive.LocalMachine, RegistryView.Registry64),
            (RegistryHive.LocalMachine, RegistryView.Registry32),
        };

        foreach (var (hive, view) in roots)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                using var kuro = baseKey.OpenSubKey(KuroKeyPath);
                if (kuro is not null)
                    CollectStrings(kuro, 0, results);
            }
            catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException or IOException)
            {
                // 无权限或注册表异常：跳过该根
            }
        }
        return results;
    }

    private static void CollectStrings(RegistryKey key, int depth, List<string> results)
    {
        foreach (var valueName in key.GetValueNames())
        {
            if (key.GetValue(valueName) is string s)
                results.Add(s);
        }

        if (depth >= MaxRegistryDepth) return;

        foreach (var subName in key.GetSubKeyNames())
        {
            try
            {
                using var sub = key.OpenSubKey(subName);
                if (sub is not null)
                    CollectStrings(sub, depth + 1, results);
            }
            catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException or IOException)
            {
                // 跳过无权限子键
            }
        }
    }
}
