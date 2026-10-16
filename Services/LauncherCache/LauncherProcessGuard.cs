using System.ComponentModel;
using System.Diagnostics;

namespace WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

/// <summary>判断鸣潮官方启动器是否在运行（运行中写缓存会在启动器退出时被覆盖）。</summary>
public static class LauncherProcessGuard
{
    /// <summary>国服启动器目录中实际存在的进程名（不含 .exe）。</summary>
    public static readonly IReadOnlyList<string> ProcessNames = new[] { "launcher_main", "launcher_updater", "KRInstallExternal" };

    public static bool IsRunning(string launcherDir) => Check(launcherDir).IsRunning;

    public static (bool IsRunning, IReadOnlyList<string> Messages) Check(string launcherDir)
    {
        var messages = new List<string>();
        var running = false;

        foreach (var name in ProcessNames)
        {
            Process[] processes;
            try
            {
                processes = Process.GetProcessesByName(name);
            }
            catch (InvalidOperationException ex)
            {
                messages.Add($"枚举进程 {name} 失败：{ex.Message}，按运行中处理");
                running = true;
                continue;
            }

            foreach (var process in processes)
            {
                using (process)
                {
                    string? path;
                    try
                    {
                        path = process.MainModule?.FileName;
                    }
                    catch (Win32Exception)
                    {
                        path = null; // 提权进程读不到路径
                    }
                    catch (InvalidOperationException)
                    {
                        continue; // 进程已退出
                    }

                    if (path is null)
                    {
                        messages.Add($"进程 {name}（PID {process.Id}）无法读取路径，按运行中处理");
                        running = true;
                    }
                    else if (LauncherCacheFiles.IsUnderDirectory(path, launcherDir))
                    {
                        messages.Add($"启动器进程 {name} 正在运行：{path}");
                        running = true;
                    }
                    else
                    {
                        messages.Add($"已忽略不在启动器目录内的同名进程：{path}");
                    }
                }
            }
        }

        return (running, messages);
    }
}
