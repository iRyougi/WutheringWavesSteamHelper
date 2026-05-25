using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WetheringWavesSteamHelper_WinUI.Models;
using WetheringWavesSteamHelper_WinUI.Services;

namespace WetheringWavesSteamHelper_WinUI.Views.Pages;

public sealed partial class CustomManifestPage : Page
{
    private readonly SteamService _steamService = new();
    private readonly SettingsService _settingsService = new();
    private readonly LogService _logService = LogService.Instance;
    private AppSettings _settings = new();

    // 页面级日志（与鸣潮页共享同一份 LogService.Logs，保持磁盘日志统一）
    private NotifyCollectionChangedEventHandler? _logScrollHandler;

    public CustomManifestPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        logList.ItemsSource = _logService.Logs;
        _logScrollHandler = (_, _) =>
        {
            logScrollViewer.ChangeView(null, logScrollViewer.ScrollableHeight, null);
        };
        _logService.Logs.CollectionChanged += _logScrollHandler;

        _settings = _settingsService.Load();
        LoadPreset(_settings.CurrentCustomManifest);
        UpdateGlobalConfigInfoBar();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_logScrollHandler != null)
            _logService.Logs.CollectionChanged -= _logScrollHandler;
    }

    private void LoadPreset(CustomManifestPreset preset)
    {
        txtAppId.Text = preset.AppId;
        txtDepotId.Text = preset.DepotId;
        txtDisplayName.Text = preset.GameDisplayName;
        txtInstallDir.Text = preset.InstallDir;
        txtClientExePath.Text = preset.ClientExePath;
        txtBuildId.Text = preset.BuildId;
        txtManifest.Text = preset.Manifest;

        var langTag = string.IsNullOrEmpty(preset.Language) ? "schinese" : preset.Language;
        foreach (var item in cmbLanguageCode.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag is string tag && tag == langTag)
            {
                cmbLanguageCode.SelectedItem = item;
                return;
            }
        }
        cmbLanguageCode.SelectedIndex = 0;
    }

    private void UpdateGlobalConfigInfoBar()
    {
        var missing = string.IsNullOrWhiteSpace(_settings.SteamInstallPath)
                   || string.IsNullOrWhiteSpace(_settings.SteamLibraryPath)
                   || string.IsNullOrWhiteSpace(_settings.SteamId);
        globalConfigInfoBar.IsOpen = missing;
    }

    // ── 事件处理（Step 5 实装具体逻辑） ──────────────────────────────────────

    private void BrowseClientExe_Click(object sender, RoutedEventArgs e) { }
    private void ClearClientExe_Click(object sender, RoutedEventArgs e) { }
    private void FetchFromSteamDB_Click(object sender, RoutedEventArgs e) { }
    private void Generate_Click(object sender, RoutedEventArgs e) { }
    private void CopyCommand_Click(object sender, RoutedEventArgs e) { }
    private void Save_Click(object sender, RoutedEventArgs e) { }
    private void ClearLog_Click(object sender, RoutedEventArgs e) => _logService.Clear();
}
