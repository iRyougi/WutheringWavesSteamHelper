using System.Collections.ObjectModel;
using System.Text;

namespace WetheringWavesSteamHelper_WinUI.Services;

public class LogService
{
    private static readonly Lazy<LogService> _instance = new(() => new LogService());
    public static LogService Instance => _instance.Value;

    private readonly string _logFilePath;
    private readonly object _writeLock = new();
    private const int MaxInMemoryLogs = 1000;

    public ObservableCollection<string> Logs { get; } = new();

    private LogService()
    {
        var logDir = ResolveLogDirectory();
        CleanupOldLogs(logDir, keep: 20);
        _logFilePath = Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
        AppendToFile($"==== Session Start: {AppInfo.FullVersion} @ {DateTime.Now:O} ====");
    }

    public void AddLog(string message)
    {
        var entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
        Logs.Add(entry);

        if (Logs.Count > MaxInMemoryLogs)
            Logs.RemoveAt(0);

        AppendToFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    public void Clear()
    {
        Logs.Clear();
    }

    private static string ResolveLogDirectory()
    {
        var exeDir = Path.Combine(AppContext.BaseDirectory, "logs");
        try
        {
            Directory.CreateDirectory(exeDir);
            var probe = Path.Combine(exeDir, ".writetest");
            File.WriteAllText(probe, "");
            File.Delete(probe);
            return exeDir;
        }
        catch
        {
            var fallback = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WetheringWavesSteamHelper_WinUI", "logs");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }

    private static void CleanupOldLogs(string directory, int keep)
    {
        try
        {
            var stale = Directory.GetFiles(directory, "*.log")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .Skip(keep);
            foreach (var f in stale)
            {
                try { File.Delete(f); } catch { }
            }
        }
        catch { }
    }

    private void AppendToFile(string line)
    {
        lock (_writeLock)
        {
            try
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }
    }
}
