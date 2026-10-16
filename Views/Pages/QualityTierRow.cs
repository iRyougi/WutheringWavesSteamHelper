using Microsoft.UI.Xaml;
using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

namespace WetheringWavesSteamHelper_WinUI.Views.Pages;

/// <summary>「画质与启动器」页中一个档位行的展示数据（只读快照，刷新时整体重建）。</summary>
public sealed class QualityTierRow
{
    public QualityTierRow(BundleStatus status, bool canWrite)
    {
        Tier = status.Tier;
        Title = $"{status.Tier.DisplayName()} {status.Tier.BundleName()}";
        PathText = status.InstallDirPath ?? "未配置";
        StateText = status.State switch
        {
            BundleInstallState.Installed => "已安装",
            BundleInstallState.ResourcesMissing => "目录存在，未检测到该档位资源",
            BundleInstallState.DirectoryMissing => "目录或客户端不存在",
            _ => "未配置",
        };
        ActiveVisibility = status.IsActiveInLauncher ? Visibility.Visible : Visibility.Collapsed;
        CanEdit = canWrite;
    }

    public QualityTier Tier { get; }
    public string Title { get; }
    public string PathText { get; }
    public string StateText { get; }
    public Visibility ActiveVisibility { get; }
    public bool CanEdit { get; }
}
