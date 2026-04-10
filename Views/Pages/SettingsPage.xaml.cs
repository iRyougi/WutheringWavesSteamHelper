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

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
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

        _isLoading = false;
    }

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
