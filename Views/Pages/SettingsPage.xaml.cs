using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WetheringWavesSteamHelper_WinUI.Models;
using WetheringWavesSteamHelper_WinUI.Services;

namespace WetheringWavesSteamHelper_WinUI.Views.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly SettingsService _settingsService = new();
    private readonly SteamService _steamService = new();
    private readonly LogService _logService = LogService.Instance;
    private AppSettings _settings = new();

    // 防止 UI 初始化时触发 Toggled / LostFocus 事件
    private bool _isLoading = true;

    // 保存最新版本信息（用于下载跳转）
    private string _downloadUrl = "";

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoading = true;

        // 从 AppInfo 填充版本信息
        txtAppName.Text = AppInfo.AppName;
        txtAppVersion.Text = $"版本 {AppInfo.FullVersion}";
        txtCopyright.Text = AppInfo.Copyright;

        _settings = _settingsService.Load();

        // 加载 Steam 全局配置（v2.2.0 起在此页面统一管理）
        txtSteamPath.Text = _settings.SteamInstallPath;
        txtLibraryPath.Text = _settings.SteamLibraryPath;
        txtSteamId.Text = _settings.SteamId;

        // 若 Steam 路径与库路径为空，尝试自动检测填入
        AutoDetectGlobalPaths();

        tglDeveloperMode.IsOn = _settings.DeveloperMode;
        tglDebugMode.IsOn = _settings.DebugMode;
        tglBetaChannel.IsOn = _settings.BetaChannel;

        // 设置语言选项（当前预留，ComboBox 禁用）
        SetLanguageSelection(_settings.Language);

        // 开发者模式相关 UI 状态
        UpdateDeveloperPanel();

        // 填充设置文件路径
        var settingsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WutheringWavesSteamHelper",
            "settings.json");
        runSettingsPath.Text = settingsPath;

        // 订阅更新通知事件，并回放已缓存的结果（防止检查早于页面加载完成）
        UpdateService.Instance.UpdateAvailable += OnUpdateAvailable;
        UpdateService.Instance.ReplayIfPending();

        _isLoading = false;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // 取消订阅，防止内存泄漏
        UpdateService.Instance.UpdateAvailable -= OnUpdateAvailable;
    }

    // ── Steam 全局配置：自动检测 ──────────────────────────────────────────────

    private void AutoDetectGlobalPaths()
    {
        var changed = false;

        if (string.IsNullOrWhiteSpace(txtSteamPath.Text))
        {
            var steamPath = _steamService.DetectSteamInstallPath();
            if (!string.IsNullOrEmpty(steamPath))
            {
                txtSteamPath.Text = steamPath;
                _settings.SteamInstallPath = steamPath;
                _logService.AddLog($"已自动识别 Steam 安装路径：{steamPath}");
                changed = true;
            }
        }

        if (string.IsNullOrWhiteSpace(txtLibraryPath.Text))
        {
            var libraries = _steamService.DetectSteamLibraryPaths();
            if (libraries.Count > 0)
            {
                txtLibraryPath.Text = libraries[0];
                _settings.SteamLibraryPath = libraries[0];
                _logService.AddLog($"已自动识别 SteamLibrary 路径：{libraries[0]}");
                changed = true;
            }
        }

        if (changed) SaveSettings();
    }

    // ── Steam 全局配置：手动编辑 ──────────────────────────────────────────────

    private void SteamPath_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        var value = txtSteamPath.Text.Trim();
        if (value == _settings.SteamInstallPath) return;
        _settings.SteamInstallPath = value;
        SaveSettings();
    }

    private void LibraryPath_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        var value = txtLibraryPath.Text.Trim();
        if (value == _settings.SteamLibraryPath) return;
        _settings.SteamLibraryPath = value;
        SaveSettings();
    }

    private void SteamId_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        var value = txtSteamId.Text.Trim();
        if (value == _settings.SteamId) return;
        _settings.SteamId = value;
        SaveSettings();
    }

    private void DetectSteamPath_Click(object sender, RoutedEventArgs e)
    {
        var steamPath = _steamService.DetectSteamInstallPath();
        if (steamPath != null)
        {
            txtSteamPath.Text = steamPath;
            _settings.SteamInstallPath = steamPath;
            SaveSettings();
            _logService.AddLog($"已自动识别 Steam 安装路径：{steamPath}");
        }
        else
        {
            _logService.AddLog("未检测到 Steam 安装路径，请手动选择");
        }
    }

    private async void BrowseSteamPath_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder == null) return;

        // 验证选择的文件夹中是否有 steam.exe
        var steamExe = System.IO.Path.Combine(folder.Path, "steam.exe");
        if (!System.IO.File.Exists(steamExe))
        {
            var dialog = new ContentDialog
            {
                Title = "提示",
                Content = $"所选文件夹中未找到 steam.exe，是否仍然使用该路径？\n{folder.Path}",
                PrimaryButtonText = "使用",
                CloseButtonText = "取消",
                XamlRoot = XamlRoot
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        }

        txtSteamPath.Text = folder.Path;
        _settings.SteamInstallPath = folder.Path;
        SaveSettings();
        _logService.AddLog($"已手动选择 Steam 安装路径：{folder.Path}");
    }

    private async void DetectLibraryPath_Click(object sender, RoutedEventArgs e)
    {
        var libraries = _steamService.DetectSteamLibraryPaths();
        if (libraries.Count == 0)
        {
            _logService.AddLog("未检测到 Steam 游戏库路径，请手动选择");
            await ShowInfoAsync("未检测到 Steam 游戏库路径，请手动选择。");
            return;
        }

        if (libraries.Count == 1)
        {
            txtLibraryPath.Text = libraries[0];
            _settings.SteamLibraryPath = libraries[0];
            SaveSettings();
            _logService.AddLog($"已自动识别 SteamLibrary 路径：{libraries[0]}");
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
            _settings.SteamLibraryPath = selectedPath;
            SaveSettings();
            _logService.AddLog($"已选择 SteamLibrary 路径：{selectedPath}");
        }
    }

    private async void BrowseLibraryPath_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder == null) return;

        var steamappsPath = System.IO.Path.Combine(folder.Path, "steamapps");
        if (!System.IO.Directory.Exists(steamappsPath))
        {
            var dialog = new ContentDialog
            {
                Title = "提示",
                Content = $"所选文件夹中未找到 steamapps 目录，是否仍然使用该路径？\n{folder.Path}",
                PrimaryButtonText = "使用",
                CloseButtonText = "取消",
                XamlRoot = XamlRoot
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        }

        txtLibraryPath.Text = folder.Path;
        _settings.SteamLibraryPath = folder.Path;
        SaveSettings();
        _logService.AddLog($"已手动选择 SteamLibrary 路径：{folder.Path}");
    }

    // ── 更新通知处理 ──────────────────────────────────────────────────────────

    private void OnUpdateAvailable(string message, string downloadUrl, bool forceUpdate)
    {
        _downloadUrl = downloadUrl;

        // 必须回到 UI 线程更新界面
        DispatcherQueue.TryEnqueue(() =>
        {
            txtUpdateMessage.Text = string.IsNullOrWhiteSpace(message)
                ? "发现新版本，点击立即下载。"
                : message;
            updateCard.Visibility = Visibility.Visible;
        });
    }

    // ── 手动检查更新 ──────────────────────────────────────────────────────────

    private async void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        btnCheckUpdate.IsEnabled = false;
        btnCheckUpdate.Content = "检查中…";
        txtCheckUpdateStatus.Text = "正在连接服务器…";

        var hadUpdate = false;

        void LocalHandler(string msg, string url, bool force) => hadUpdate = true;
        UpdateService.Instance.UpdateAvailable += LocalHandler;

        try
        {
            await UpdateService.Instance.CheckUpdateAsync().ConfigureAwait(false);
        }
        finally
        {
            UpdateService.Instance.UpdateAvailable -= LocalHandler;
        }

        DispatcherQueue.TryEnqueue(() =>
        {
            btnCheckUpdate.IsEnabled = true;
            btnCheckUpdate.Content = "检查更新";

            var gateUntil = UpdateService.Instance.PendingGateUntil;
            txtCheckUpdateStatus.Text = hadUpdate
                ? "已发现新版本，请查看上方通知。"
                : gateUntil is not null
                    ? $"新版本计划于 {gateUntil.Value.LocalDateTime:yyyy-MM-dd HH:mm} 开放更新，请稍后再试。"
                    : "当前已是最新版本。";
        });
    }

    // ── 立即下载 ──────────────────────────────────────────────────────────────

    private async void BtnDownload_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_downloadUrl)) return;

        try
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(_downloadUrl));
        }
        catch
        {
            // 静默忽略
        }
    }

    // ── 开发者模式 ────────────────────────────────────────────────────────────

    private void DeveloperMode_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;

        _settings.DeveloperMode = tglDeveloperMode.IsOn;
        UpdateDeveloperPanel();
        SaveSettings();
    }

    private void DebugMode_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;

        _settings.DebugMode = tglDebugMode.IsOn;
        SaveSettings();
    }

    private void BetaChannel_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;

        _settings.BetaChannel = tglBetaChannel.IsOn;
        SaveSettings();
    }

    private void Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading) return;
        if (cmbLanguage.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            _settings.Language = tag;
            SaveSettings();
        }
    }

    private async void OpenSettingsDir_Click(object sender, RoutedEventArgs e)
    {
        var settingsDir = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WutheringWavesSteamHelper");

        try
        {
            if (!System.IO.Directory.Exists(settingsDir))
                System.IO.Directory.CreateDirectory(settingsDir);

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = settingsDir,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = "错误",
                Content = $"无法打开目录：{ex.Message}",
                CloseButtonText = "确定",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    private void UpdateDeveloperPanel()
    {
        developerOptionsPanel.Visibility = tglDeveloperMode.IsOn
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void SetLanguageSelection(string language)
    {
        foreach (var item in cmbLanguage.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag is string tag && tag == language)
            {
                cmbLanguage.SelectedItem = item;
                return;
            }
        }
        // 默认选中简体中文
        if (cmbLanguage.Items.Count > 0)
            cmbLanguage.SelectedIndex = 0;
    }

    private void SaveSettings()
    {
        _settingsService.Save(_settings);
    }

    private async Task ShowInfoAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "提示",
            Content = message,
            CloseButtonText = "确定",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }
}
