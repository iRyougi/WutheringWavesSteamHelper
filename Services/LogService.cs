using System.Collections.ObjectModel;

namespace WetheringWavesSteamHelper_WinUI.Services;

public class LogService
{
    public ObservableCollection<string> Logs { get; } = new();

    public void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Logs.Add($"[{timestamp}] {message}");
    }

    public void Clear()
    {
        Logs.Clear();
    }
}
