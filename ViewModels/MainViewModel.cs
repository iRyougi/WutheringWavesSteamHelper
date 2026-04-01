using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WutheringWavesSteamHelper.Models;
using WutheringWavesSteamHelper.Services;

namespace WutheringWavesSteamHelper.ViewModels;

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

    [RelayCommand]
    private void GenerateConfig()
    {
        if (string.IsNullOrWhiteSpace(SteamLibraryPath) || string.IsNullOrWhiteSpace(SteamInstallPath) ||
            string.IsNullOrWhiteSpace(SteamId) || string.IsNullOrWhiteSpace(BuildId) || string.IsNullOrWhiteSpace(Manifest))
        {
            AddLog("[错误] 请填写所有必填项");
            return;
        }

        try
        {
            var libraryPath = SteamLibraryPath;
            var steamappsPath = Path.Combine(libraryPath, "steamapps");
            var commonPath = Path.Combine(steamappsPath, "common");
            var gamePath = Path.Combine(commonPath, "Wuthering Waves");
            var launcherPath = Path.Combine(SteamInstallPath, "steam.exe");

            var acfPath = Path.Combine(steamappsPath, "appmanifest_3513350.acf");
            var exePath = Path.Combine(gamePath, "Wuthering Waves.exe");

            if (!Directory.Exists(steamappsPath))
                Directory.CreateDirectory(steamappsPath);

            var acfContent = SteamHelper.GenerateAcfContent(launcherPath, BuildId.Trim(), SteamId.Trim(), Manifest.Trim());
            File.WriteAllText(acfPath, acfContent);
            AddLog($"已生成：{acfPath}");

            if (!Directory.Exists(gamePath))
                Directory.CreateDirectory(gamePath);

            if (!File.Exists(exePath))
            {
                File.Create(exePath).Dispose();
                AddLog($"已创建占位 EXE：{exePath}");
            }

            SaveSettings();
            AddLog("[完成] 全部操作已完成，请重启 Steam。");
        }
        catch (Exception ex)
        {
            AddLog($"[错误] 操作失败：{ex.Message}");
        }
    }

    public string? GenerateLaunchCommandText()
    {
        string? selectedPath = null;

        if (IsWeGame)
        {
            selectedPath = SteamHelper.DetectWeGameInstallPath();
        }
        else
        {
            var paths = SteamHelper.DetectCnWutheringWavesPaths()
                .Where(p => File.Exists(Path.Combine(p, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe")))
                .ToList();
            if (paths.Count > 0)
                selectedPath = paths[0];
        }

        if (selectedPath != null)
        {
            SaveSettings();
            return SteamHelper.GenerateLaunchCommand(selectedPath);
        }

        return null;
    }

    public void OpenLauncher()
    {
        if (IsWeGame)
        {
            AddLog("WeGame 版鸣潮请通过 WeGame 客户端启动");
            return;
        }

        var paths = SteamHelper.DetectCnWutheringWavesPaths();
        string? launcherExe = null;

        foreach (var installPath in paths)
        {
            var candidate = Path.Combine(installPath, "launcher.exe");
            if (File.Exists(candidate))
            {
                launcherExe = candidate;
                break;
            }
        }

        if (launcherExe != null)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = launcherExe,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(launcherExe)
                });
                AddLog($"已启动官方启动器：{launcherExe}");
            }
            catch (Exception ex)
            {
                AddLog($"启动失败：{ex.Message}");
            }
        }
        else
        {
            AddLog("未找到官方启动器");
        }
    }
}
