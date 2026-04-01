using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.DataTransfer;
using WetheringWavesSteamHelper_WinUI.Services;
using WetheringWavesSteamHelper_WinUI.Models;

namespace WetheringWavesSteamHelper_WinUI.Views.Pages;

public sealed partial class WutheringWavesPage : Page
{
    private readonly SteamService _steamService = new();
    private readonly SettingsService _settingsService = new();
    private readonly LogService _logService = new();
    private AppSettings _settings = new();

    public WutheringWavesPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        logList.ItemsSource = _logService.Logs;
        _settings = _settingsService.Load();
        LoadSettings();
        AutoDetectPaths();
    }

    private void LoadSettings()
    {
        txtSteamPath.Text = _settings.SteamInstallPath;
        txtLibraryPath.Text = _settings.SteamLibraryPath;
        txtSteamId.Text = _settings.SteamId;
        txtBuildId.Text = _settings.BuildId;
        txtManifest.Text = _settings.Manifest;

        if (_settings.CnGameSource == "wegame")
            rdoWeGame.IsChecked = true;
        else
            rdoOfficial.IsChecked = true;
    }

    private void AutoDetectPaths()
    {
        if (string.IsNullOrEmpty(txtSteamPath.Text))
        {
            var steamPath = _steamService.DetectSteamInstallPath();
            if (steamPath != null)
            {
                txtSteamPath.Text = steamPath;
                _logService.AddLog($"自动检测到 Steam 路径: {steamPath}");
            }
        }

        if (string.IsNullOrEmpty(txtLibraryPath.Text))
        {
            var libraries = _steamService.DetectSteamLibraryPaths();
            if (libraries.Count > 0)
            {
                txtLibraryPath.Text = libraries[0];
                _logService.AddLog($"自动检测到游戏库路径: {libraries[0]}");
            }
        }
    }

    private async void DetectLibraryPath_Click(object sender, RoutedEventArgs e)
    {
        var libraries = _steamService.DetectSteamLibraryPaths();
        if (libraries.Count == 0)
        {
            _logService.AddLog("未检测到 Steam 游戏库路径");
            return;
        }

        if (libraries.Count == 1)
        {
            txtLibraryPath.Text = libraries[0];
            _logService.AddLog($"检测到游戏库路径: {libraries[0]}");
            return;
        }

        var comboBox = new ComboBox
        {
            ItemsSource = libraries,
            SelectedIndex = 0,
            MinWidth = 520
        };

        var dialog = new ContentDialog
        {
            Title = "检测到多个 Steam 游戏库",
            Content = comboBox,
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && comboBox.SelectedItem is string selectedPath)
        {
            txtLibraryPath.Text = selectedPath;
            _logService.AddLog($"已选择游戏库路径: {selectedPath}");
        }
    }

    private async void BrowseSteamPath_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            txtSteamPath.Text = folder.Path;
            _logService.AddLog($"已选择 Steam 路径: {folder.Path}");
        }
    }

    private async void BrowseLibraryPath_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            txtLibraryPath.Text = folder.Path;
            _logService.AddLog($"已选择游戏库路径: {folder.Path}");
        }
    }

    private void DetectSteamPath_Click(object sender, RoutedEventArgs e)
    {
        var steamPath = _steamService.DetectSteamInstallPath();
        if (steamPath != null)
        {
            txtSteamPath.Text = steamPath;
            _logService.AddLog($"检测到 Steam 路径: {steamPath}");
        }
        else
        {
            _logService.AddLog("未检测到 Steam 安装路径");
        }
    }

    private async void FetchFromSteamDB_Click(object sender, RoutedEventArgs e)
    {
        _logService.AddLog("正在从 SteamDB 获取信息...");
        var result = await _steamService.FetchSteamDbInfoAsync();

        if (result.HasValue)
        {
            txtBuildId.Text = result.Value.buildId;
            txtManifest.Text = result.Value.manifest;
            _logService.AddLog($"获取成功 - BuildID: {result.Value.buildId}");
        }
        else
        {
            _logService.AddLog("获取失败，请手动输入");
        }
    }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInputs()) return;

        try
        {
            var libraryPath = txtLibraryPath.Text;
            var steamappsPath = Path.Combine(libraryPath, "steamapps");
            Directory.CreateDirectory(steamappsPath);

            var acfPath = Path.Combine(steamappsPath, "appmanifest_3513350.acf");
            var launcherPath = Path.Combine(steamappsPath, "common", "Wuthering Waves", "Wuthering Waves.exe");

            var acfContent = _steamService.GenerateAcfContent(
                launcherPath,
                txtBuildId.Text,
                txtSteamId.Text,
                txtManifest.Text
            );

            await File.WriteAllTextAsync(acfPath, acfContent);

            var gameDir = Path.Combine(steamappsPath, "common", "Wuthering Waves");
            Directory.CreateDirectory(gameDir);
            if (!File.Exists(launcherPath))
                await File.WriteAllTextAsync(launcherPath, "");

            _logService.AddLog("配置文件生成成功！");
            _logService.AddLog($"文件位置: {acfPath}");
            _logService.AddLog("请重启 Steam 客户端");

            SaveSettings();
        }
        catch (Exception ex)
        {
            _logService.AddLog($"生成失败: {ex.Message}");
        }
    }

    private void CopyCommand_Click(object sender, RoutedEventArgs e)
    {
        var gamePaths = _steamService.DetectCnWutheringWavesPaths();
        if (gamePaths.Count == 0)
        {
            _logService.AddLog("未检测到游戏安装路径");
            return;
        }

        var command = _steamService.GenerateLaunchCommand(gamePaths[0]);
        var dataPackage = new DataPackage();
        dataPackage.SetText(command);
        Clipboard.SetContent(dataPackage);

        _logService.AddLog("启动命令已复制到剪贴板");
        _logService.AddLog($"命令: {command}");
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e)
    {
        _logService.Clear();
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(txtSteamPath.Text))
        {
            _logService.AddLog("错误: 请输入 Steam 安装路径");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtLibraryPath.Text))
        {
            _logService.AddLog("错误: 请输入游戏库路径");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtSteamId.Text))
        {
            _logService.AddLog("错误: 请输入 Steam ID");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtBuildId.Text) || string.IsNullOrWhiteSpace(txtManifest.Text))
        {
            _logService.AddLog("错误: 请输入 BuildID 和 Manifest");
            return false;
        }
        return true;
    }

    private void SaveSettings()
    {
        _settings.SteamInstallPath = txtSteamPath.Text;
        _settings.SteamLibraryPath = txtLibraryPath.Text;
        _settings.SteamId = txtSteamId.Text;
        _settings.BuildId = txtBuildId.Text;
        _settings.Manifest = txtManifest.Text;
        _settings.CnGameSource = rdoWeGame.IsChecked == true ? "wegame" : "official";
        _settingsService.Save(_settings);
    }
}
