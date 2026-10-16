using System.Diagnostics;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>
/// 在真实游戏目录旁创建名为「Wuthering Waves Game」的目录联接，
/// 使新版启动器「定位游戏」时强制拼接的子目录落到真实包体上。
/// </summary>
public static class JunctionService
{
    private const int TimeoutMs = 10_000;

    /// <summary>联接路径 = 目标目录的父目录\Wuthering Waves Game；目标为盘符根目录时返回 null。</summary>
    public static string? GetLinkPath(string targetDir)
    {
        var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(targetDir));
        var parent = Path.GetDirectoryName(full);
        return parent is null ? null : Path.Combine(parent, LauncherCacheFiles.GameDirName);
    }

    /// <summary>校验能否创建联接；可以返回 null，否则返回中文原因。</summary>
    public static string? Validate(string targetDir)
    {
        if (string.IsNullOrWhiteSpace(targetDir) || !Path.IsPathFullyQualified(targetDir))
            return "请选择完整的游戏目录路径";
        if (!Directory.Exists(targetDir))
            return $"目录不存在：{targetDir}";
        if (!File.Exists(LauncherCacheFiles.ClientExePath(targetDir)))
            return $"所选目录不是游戏目录（未找到 {LauncherCacheFiles.ClientExePath(targetDir)}）";

        var name = Path.GetFileName(Path.TrimEndingDirectorySeparator(Path.GetFullPath(targetDir)));
        if (name.Equals(LauncherCacheFiles.GameDirName, StringComparison.OrdinalIgnoreCase))
            return $"所选目录已经叫「{LauncherCacheFiles.GameDirName}」，不需要创建联接，直接在启动器中定位其父目录即可";

        var link = GetLinkPath(targetDir);
        if (link is null)
            return "不能对磁盘根目录创建联接";
        if (Directory.Exists(link) || File.Exists(link))
            return $"目标位置已存在同名文件或目录：{link}";

        return null;
    }

    public static JunctionResult Create(string targetDir)
    {
        var error = Validate(targetDir);
        var link = GetLinkPath(targetDir) ?? string.Empty;
        if (error is not null)
            return new JunctionResult(false, link, error);

        var target = Path.TrimEndingDirectorySeparator(Path.GetFullPath(targetDir));
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/d /v:off /c mklink /J \"%WWSH_JUNCTION_LINK%\" \"%WWSH_JUNCTION_TARGET%\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        // cmd 只展开命令中的变量一次，变量值里的 %NAME% 和 !NAME! 保持路径字面内容。
        psi.Environment["WWSH_JUNCTION_LINK"] = link;
        psi.Environment["WWSH_JUNCTION_TARGET"] = target;

        try
        {
            using var process = Process.Start(psi);
            if (process is null)
                return new JunctionResult(false, link, "无法启动 cmd.exe");

            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(TimeoutMs))
            {
                process.Kill();
                return new JunctionResult(false, link, "创建联接超时");
            }

            var output = $"{stdout.Result}{stderr.Result}".Trim();
            return process.ExitCode == 0 && Directory.Exists(link)
                ? new JunctionResult(true, link, $"已创建联接：{link} → {target}")
                : new JunctionResult(false, link, $"创建联接失败（退出码 {process.ExitCode}）：{output}");
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or IOException)
        {
            return new JunctionResult(false, link, $"创建联接失败：{ex.Message}");
        }
    }
}
