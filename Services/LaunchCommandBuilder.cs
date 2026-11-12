using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

namespace WetheringWavesSteamHelper_WinUI.Services;

/// <summary>Steam 启动选项生成。</summary>
public static class LaunchCommandBuilder
{
    /// <summary>
    /// 生成 "exe" -krqlv=档位 %command%。画质参数必须在 %command% 之前：
    /// 命令行出现多个 -krqlv= 时最前面的生效（鸣潮 3.8 前 Steam 展开 %command% 时自带 -krqlv=hd；3.8 起启动项改为 launcher.exe，不再附带参数）。
    /// </summary>
    public static string Build(string exePath, QualityTier tier) =>
        $"\"{exePath}\" -krqlv={tier.KrqlvValue()} %command%";
}
