using System;
using System.IO;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>启动器缓存相关的路径约定与判定。</summary>
public static class LauncherCacheFiles
{
    public const string CacheDirName = "kr_game_cache";
    public const string GameTempFileName = "kr_game_temp.bin";
    public const string UiStateFileName = "platform_ui_state.bin";
    public const string GameDirName = "Wuthering Waves Game";
    public const string LauncherExeName = "launcher.exe";

    public static string GameTempPath(string launcherDir) => Path.Combine(launcherDir, CacheDirName, GameTempFileName);

    public static string UiStatePath(string launcherDir) => Path.Combine(launcherDir, CacheDirName, UiStateFileName);

    public static string ClientExePath(string gameDir) =>
        Path.Combine(gameDir, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");

    /// <summary>目录下存在 kr_game_cache\kr_game_temp.bin 即视为新版启动器目录。</summary>
    public static bool IsLauncherDir(string? dir) =>
        !string.IsNullOrWhiteSpace(dir) && File.Exists(GameTempPath(dir));

    public static BundleInstallState GetInstallState(QualityTier tier, string? gameDir)
    {
        if (string.IsNullOrWhiteSpace(gameDir))
            return BundleInstallState.NotConfigured;
        if (!Directory.Exists(gameDir) || !File.Exists(ClientExePath(gameDir)))
            return BundleInstallState.DirectoryMissing;
        return Directory.Exists(Path.Combine(gameDir, tier.ResourceMarkerRelativePath()))
            ? BundleInstallState.Installed
            : BundleInstallState.ResourcesMissing;
    }

    /// <summary>path 是否位于 dir 之内（含 dir 本身），不区分大小写。</summary>
    public static bool IsUnderDirectory(string path, string dir)
    {
        var fullPath = Path.GetFullPath(path);
        var fullDir = Path.TrimEndingDirectorySeparator(Path.GetFullPath(dir));
        return fullPath.Equals(fullDir, StringComparison.OrdinalIgnoreCase)
            || fullPath.StartsWith(fullDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
