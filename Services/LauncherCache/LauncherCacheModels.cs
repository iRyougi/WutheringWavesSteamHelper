using System;
using System.Collections.Generic;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>启动器缓存文件格式不符合预期。</summary>
public sealed class KrCacheFormatException : Exception
{
    public KrCacheFormatException(string message) : base(message) { }
    public KrCacheFormatException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>kr_game_temp.bin 中 resources 数组的一项。</summary>
public sealed record BundleEntry(string BundleName, QualityTier? Tier, string? ResId, string InstallDirPath);

public enum BundleInstallState
{
    /// <summary>缓存里没有该档位的记录。</summary>
    NotConfigured,
    /// <summary>记录的目录或客户端 exe 不存在。</summary>
    DirectoryMissing,
    /// <summary>客户端存在，但该档位的资源标记目录不存在。</summary>
    ResourcesMissing,
    Installed,
}

public sealed record BundleStatus(
    QualityTier Tier,
    string? InstallDirPath,
    BundleInstallState State,
    bool IsActiveInLauncher)
{
    /// <summary>该档位游戏目录下的 Client-Win64-Shipping.exe 完整路径；未配置时为 null。</summary>
    public string? ClientExePath => InstallDirPath is null ? null : LauncherCacheFiles.ClientExePath(InstallDirPath);
}

public sealed record LauncherStatus(
    IReadOnlyList<string> CandidateLauncherDirs,
    string? LauncherDir,
    IReadOnlyList<BundleStatus> Bundles,
    QualityTier? ActiveBundle,
    bool CanWrite,
    bool IsLauncherRunning,
    string? Error)
{
    public static LauncherStatus NotFound(IReadOnlyList<string> candidates) =>
        new(candidates, null, Array.Empty<BundleStatus>(), null, false, false, "未找到新版官方启动器（需鸣潮 3.7 及以上的启动器）");
}

public sealed record WriteResult(bool Success, string? BackupPath, IReadOnlyList<string> Messages);

public sealed record BackupInfo(string FilePath, DateTime CreatedAt, long SizeBytes);

public sealed record JunctionResult(bool Success, string LinkPath, string Message);
