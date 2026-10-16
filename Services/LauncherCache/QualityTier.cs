using System;
using System.Collections.Generic;
using System.IO;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>鸣潮 3.7 起的客户端资源分级档位。</summary>
public enum QualityTier
{
    UHD,
    HD,
    SD,
}

public static class QualityTiers
{
    /// <summary>按推荐优先级排列的全部档位。</summary>
    public static IReadOnlyList<QualityTier> All { get; } = new[] { QualityTier.UHD, QualityTier.HD, QualityTier.SD };

    /// <summary>启动参数 -krqlv= 的取值。</summary>
    public static string KrqlvValue(this QualityTier tier) => tier switch
    {
        QualityTier.UHD => "uhd",
        QualityTier.HD => "hd",
        QualityTier.SD => "sd",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null),
    };

    /// <summary>启动器缓存中 bundleName 的取值。</summary>
    public static string BundleName(this QualityTier tier) => tier.ToString();

    /// <summary>界面显示用中文名。</summary>
    public static string DisplayName(this QualityTier tier) => tier switch
    {
        QualityTier.UHD => "极致",
        QualityTier.HD => "高清",
        QualityTier.SD => "流畅",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null),
    };

    /// <summary>判断该档位资源是否已安装的标记目录（相对游戏目录）。</summary>
    public static string ResourceMarkerRelativePath(this QualityTier tier) => tier switch
    {
        QualityTier.UHD => Path.Combine("Client", "Content", "UHD"),
        QualityTier.HD => Path.Combine("Client", "Content", "HD"),
        QualityTier.SD => Path.Combine("Client", "Content", "Paks"),
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null),
    };

    /// <summary>宽松解析：接受 UHD/HD/SD（不区分大小写）与中文名；无法识别返回 null。</summary>
    public static QualityTier? Parse(string? value)
    {
        var v = value?.Trim();
        if (string.IsNullOrEmpty(v)) return null;

        return v.ToUpperInvariant() switch
        {
            "UHD" or "极致" => QualityTier.UHD,
            "HD" or "高清" => QualityTier.HD,
            "SD" or "流畅" or "标准" or "标清" => QualityTier.SD,
            _ => null,
        };
    }
}
