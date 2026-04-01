using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WutheringWavesSteamHelper.WinUI.Models;
using WutheringWavesSteamHelper.WinUI.Services;

namespace WutheringWavesSteamHelper.WinUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string steamLibraryPath = "";

    [ObservableProperty]
    private string steamInstallPath = "";

    [ObservableProperty]
    private string steamId = "";

    [ObservableProperty]
    private string buildId = "";

    [ObservableProperty]
    private string manifest = "";

    [ObservableProperty]
    private bool isOfficial = true;

    [ObservableProperty]
    private bool isWeGame = false;

    [ObservableProperty]
    private bool isFetching = false;

    public ObservableCollection<string> Logs { get; } = new();

    private AppSettings _settings = new();

    public MainViewModel()
    {
        LoadSettings();
        AutoDetectPaths();
    }

    private void LoadSettings()
    {
        _settings = AppSettings.Load();
        SteamLibraryPath = _settings.SteamLibraryPath;
        SteamInstallPath = _settings.SteamInstallPath;
        SteamId = _settings.SteamId;
        BuildId = _settings.BuildId;
        Manifest = _settings.Manifest;

        if (_settings.CnGameSource == "wegame")
        {
            IsWeGame = true;
            IsOfficial = false;
        }
    }

    private void SaveSettings()
    {
        _settings.SteamLibraryPath = SteamLibraryPath;
        _settings.SteamInstallPath = SteamInstallPath;
        _settings.SteamId = SteamId;
        _settings.BuildId = BuildId;
        _settings.Manifest = Manifest;
        _settings.CnGameSource = IsWeGame ? "wegame" : "official";
        _settings.Save();
    }

    private void AutoDetectPaths()
    {
        if (string.IsNullOrEmpty(SteamInstallPath))
        {
            var steamPath = SteamHelper.DetectSteamInstallPath();
            if (!string.IsNullOrEmpty(steamPath))
            {
                SteamInstallPath = steamPath;
                AddLog($"已自动识别 Steam 安装路径：{steamPath}");
            }
        }

        if (string.IsNullOrEmpty(SteamLibraryPath))
        {
            var libraryPaths = SteamHelper.DetectSteamLibraryPaths();
            if (libraryPaths.Count > 0)
            {
                SteamLibraryPath = libraryPaths[0];
                AddLog($"已自动识别 SteamLibrary 路径：{libraryPaths[0]}");
            }
        }
    }

    public void AddLog(string message)
    {
        Logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    [RelayCommand]
    private async Task FetchSteamDbAsync()
    {
        IsFetching = true;
        AddLog("正在从 SteamDB 获取 BuildID 和 Manifest...");

        try
        {
            var result = await SteamHelper.FetchSteamDbInfoAsync();
            if (result.HasValue)
            {
                BuildId = result.Value.buildId;
                Manifest = result.Value.manifest;
                AddLog($"获取成功：BuildID: {result.Value.buildId}, Manifest: {result.Value.manifest}");
            }
            else
            {
                AddLog("获取失败，请检查网络连接或手动填写");
            }
        }
        catch (Exception ex)
        {
            AddLog($"获取异常：{ex.Message}");
        }
        finally
        {
            IsFetching = false;
        }
    }

    [RelayCommand]
    private void AutoDetectSteam()
    {
        var path = SteamHelper.DetectSteamInstallPath();
        if (!string.IsNullOrEmpty(path))
        {
            SteamInstallPath = path;
            AddLog($"已自动识别 Steam 安装路径：{path}");
        }
        else
        {
            AddLog("未能自动识别 Steam 安装路径");
        }
    }

    [RelayCommand]
    private void AutoDetectLibrary()
    {
        var paths = SteamHelper.DetectSteamLibraryPaths();
        if (paths.Count > 0)
        {
            SteamLibraryPath = paths[0];
            AddLog($"已自动识别 SteamLibrary 路径：{paths[0]}");
        }
        else
        {
            AddLog("未能自动识别 SteamLibrary 路径");
        }
    }
}
