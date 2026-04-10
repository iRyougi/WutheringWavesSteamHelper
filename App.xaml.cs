using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using System.Text;
using WetheringWavesSteamHelper_WinUI.Services;

namespace WetheringWavesSteamHelper_WinUI;

public partial class App : Application
{
    public static Window MainWindow { get; private set; } = null!;
    private static readonly string CrashLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WetheringWavesSteamHelper_WinUI",
        "logs",
        "startup-crash.log");
    private static readonly string CrashLogFallbackPath = Path.Combine(
        AppContext.BaseDirectory,
        "startup-crash.log");

    public App()
    {
        WriteCrashLog("App.Constructor.Start", null);
        RegisterGlobalExceptionHandlers();
        InitializeComponent();
        WriteCrashLog("App.Constructor.End", null);
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();

            // 后台静默检查更新，不阻塞启动
            _ = Task.Run(async () =>
            {
                try
                {
                    await UpdateService.Instance.CheckUpdateAsync().ConfigureAwait(false);
                }
                catch
                {
                    // 静默忽略，不影响主流程
                }
            });
        }
        catch (Exception ex)
        {
            WriteCrashLog("OnLaunched", ex);
            throw;
        }
    }

    private void RegisterGlobalExceptionHandlers()
    {
        UnhandledException += OnApplicationUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnApplicationUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        WriteCrashLog("Application.UnhandledException", e.Exception);
    }

    private void OnCurrentDomainUnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            WriteCrashLog("AppDomain.CurrentDomain.UnhandledException", ex);
        }
        else
        {
            WriteCrashLog("AppDomain.CurrentDomain.UnhandledException", null, e.ExceptionObject?.ToString());
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        WriteCrashLog("TaskScheduler.UnobservedTaskException", e.Exception);
    }

    private static void WriteCrashLog(string source, Exception? ex, string? raw = null)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("==============================");
            sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Source: {source}");
            sb.AppendLine($"PrimaryPath: {CrashLogPath}");
            sb.AppendLine($"FallbackPath: {CrashLogFallbackPath}");

            if (ex != null)
            {
                sb.AppendLine($"Exception: {ex.GetType().FullName}");
                sb.AppendLine($"Message: {ex.Message}");
                sb.AppendLine($"HResult: 0x{ex.HResult:X8}");
                if (ex is COMException comEx)
                {
                    sb.AppendLine($"COM ErrorCode: 0x{comEx.ErrorCode:X8}");
                }

                sb.AppendLine("StackTrace:");
                sb.AppendLine(ex.StackTrace ?? "<null>");

                if (ex.InnerException != null)
                {
                    sb.AppendLine("InnerException:");
                    sb.AppendLine(ex.InnerException.ToString());
                }
            }
            else
            {
                sb.AppendLine("Exception: <null>");
            }

            if (!string.IsNullOrWhiteSpace(raw))
            {
                sb.AppendLine("Raw:");
                sb.AppendLine(raw);
            }

            var content = sb.ToString();
            if (!TryAppendText(CrashLogPath, content))
            {
                TryAppendText(CrashLogFallbackPath, content);
            }
        }
        catch
        {
        }
    }

    private static bool TryAppendText(string path, string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(path, content, Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
