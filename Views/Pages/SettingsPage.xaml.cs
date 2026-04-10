using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WetheringWavesSteamHelper_WinUI.Models;
using WetheringWavesSteamHelper_WinUI.Services;

namespace WetheringWavesSteamHelper_WinUI.Views.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly SettingsService _settingsService = new();
    private AppSettings _settings = new();

    // 防止 UI 初始化时触发 Toggled 事件
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
            txtCheckUpdateStatus.Text = hadUpdate ? "已发现新版本，请查看上方通知。" : "当前已是最新版本。";
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
}

