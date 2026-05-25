using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Specialized;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using WetheringWavesSteamHelper_WinUI.Models;
using WetheringWavesSteamHelper_WinUI.Services;

namespace WetheringWavesSteamHelper_WinUI.Views.Pages;

public sealed partial class CustomManifestPage : Page
{
    private readonly SteamService _steamService = new();
    private readonly SettingsService _settingsService = new();
    private readonly LogService _logService = LogService.Instance;
    private AppSettings _settings = new();

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

    private CustomManifestPreset BuildPresetFromUI()
    {
        var langTag = (cmbLanguageCode.SelectedItem as ComboBoxItem)?.Tag as string ?? "schinese";
        return new CustomManifestPreset
        {
            Name = string.IsNullOrWhiteSpace(_settings.CurrentCustomManifest.Name)
                ? "默认"
                : _settings.CurrentCustomManifest.Name,
            AppId = txtAppId.Text.Trim(),
            DepotId = txtDepotId.Text.Trim(),
            BuildId = txtBuildId.Text.Trim(),
            Manifest = txtManifest.Text.Trim(),
            GameDisplayName = txtDisplayName.Text.Trim(),
            InstallDir = txtInstallDir.Text.Trim(),
            ClientExePath = txtClientExePath.Text.Trim(),
            Language = langTag,
        };
    }

    // ── 浏览 EXE / 清除 ───────────────────────────────────────────────────────

    private async void BrowseClientExe_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add(".exe");
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

        var file = await picker.PickSingleFileAsync();
        if (file == null) return;

        txtClientExePath.Text = file.Path;
        _logService.AddLog($"[自定义页] 已选择可执行文件：{file.Path}");
    }

    private void ClearClientExe_Click(object sender, RoutedEventArgs e)
    {
        txtClientExePath.Text = string.Empty;
        _logService.AddLog("[自定义页] 已清除可执行文件路径");
    }

    // ── SteamDB 获取（基于用户输入的 AppID / DepotID） ────────────────────────

    private async void FetchFromSteamDB_Click(object sender, RoutedEventArgs e)
    {
        var appId = txtAppId.Text.Trim();
        var depotId = txtDepotId.Text.Trim();

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(depotId))
        {
            await ShowInfoAsync("请先填写 AppID 和 DepotID。");
            return;
        }
        if (!long.TryParse(appId, out _) || !long.TryParse(depotId, out _))
        {
            await ShowInfoAsync("AppID 和 DepotID 必须为数字。");
            return;
        }

        btnFetchSteamDB.IsEnabled = false;
        var originalContent = btnFetchSteamDB.Content;
        btnFetchSteamDB.Content = "正在获取...";

        _logService.AddLog($"[自定义页] 正在从 SteamDB 获取 AppID={appId} DepotID={depotId} 的 BuildID 和 Manifest...");

        try
        {
            var result = await _steamService.FetchSteamDbInfoAsync(appId, depotId);
            if (result.HasValue)
            {
                txtBuildId.Text = result.Value.buildId;
                txtManifest.Text = result.Value.manifest;
                _logService.AddLog($"[自定义页] 获取成功 - BuildID: {result.Value.buildId}, Manifest: {result.Value.manifest}");
            }
            else
            {
                _logService.AddLog("[自定义页] 获取失败，请检查 AppID/DepotID 或网络连接");
                await ShowInfoAsync($"无法从 SteamDB 获取信息。\n\n请检查 AppID/DepotID 是否正确，或访问 https://steamdb.info/app/{appId}/depots/ 手动查询后填写。");
            }
        }
        catch (Exception ex)
        {
            _logService.AddLog($"[自定义页] 获取异常：{ex.Message}");
        }
        finally
        {
            btnFetchSteamDB.IsEnabled = true;
            btnFetchSteamDB.Content = originalContent;
        }
    }

    // ── 生成 ACF ──────────────────────────────────────────────────────────────

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        // 重新加载设置，确保全局字段是最新的
        _settings = _settingsService.Load();
        UpdateGlobalConfigInfoBar();

        if (!ValidateForGenerate()) return;

        btnGenerate.IsEnabled = false;
        try
        {
            var appId = txtAppId.Text.Trim();
            var depotId = txtDepotId.Text.Trim();
            var displayName = txtDisplayName.Text.Trim();
            var installDir = txtInstallDir.Text.Trim();
            var buildId = txtBuildId.Text.Trim();
            var manifest = txtManifest.Text.Trim();
            var langTag = (cmbLanguageCode.SelectedItem as ComboBoxItem)?.Tag as string ?? "schinese";

            var libraryPath = _settings.SteamLibraryPath.Trim();
            var steamInstallPath = _settings.SteamInstallPath.Trim();
            var steamappsPath = Path.Combine(libraryPath, "steamapps");
            var launcherPath = Path.Combine(steamInstallPath, "steam.exe");
            var acfPath = Path.Combine(steamappsPath, $"appmanifest_{appId}.acf");

            if (File.Exists(acfPath))
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "文件已存在",
                    Content = $"检测到配置文件已存在：\nappmanifest_{appId}.acf\n\n是否要覆盖该文件？",
                    PrimaryButtonText = "覆盖",
                    CloseButtonText = "取消",
                    XamlRoot = XamlRoot
                };
                if (await confirmDialog.ShowAsync() != ContentDialogResult.Primary)
                {
                    _logService.AddLog("[自定义页] 用户取消覆盖 ACF 文件，操作已跳过。");
                    return;
                }
                _logService.AddLog("[自定义页] 用户选择覆盖 ACF 文件。");
            }

            var acfContent = _steamService.GenerateAcfContent(new AcfParameters(
                AppId: appId,
                DepotId: depotId,
                LauncherPath: launcherPath,
                DisplayName: displayName,
                InstallDir: installDir,
                BuildId: buildId,
                LastOwner: _settings.SteamId.Trim(),
                Manifest: manifest,
                Language: langTag));

            if (!Directory.Exists(steamappsPath))
            {
                Directory.CreateDirectory(steamappsPath);
                _logService.AddLog($"[自定义页] 已创建目录：{steamappsPath}");
            }

            await File.WriteAllTextAsync(acfPath, acfContent);
            _logService.AddLog($"[自定义页] 已生成：{acfPath}");

            // 同步保存当前配置（与鸣潮页一致的隐式保存语义）
            PersistCurrentPreset();
            _logService.AddLog("[自定义页][完成] ACF 已生成，请重启 Steam。");

            await ShowInfoAsync($"配置生成成功！\n\n请重启 Steam 客户端，然后在库中找到「{displayName}」。");
        }
        catch (Exception ex)
        {
            _logService.AddLog($"[自定义页][错误] 操作失败：{ex.Message}");
            await ShowInfoAsync($"生成过程中出现错误：\n{ex.Message}");
        }
        finally
        {
            btnGenerate.IsEnabled = true;
        }
    }

    // ── 复制启动命令 ──────────────────────────────────────────────────────────

    private async void CopyCommand_Click(object sender, RoutedEventArgs e)
    {
        var exePath = txtClientExePath.Text.Trim();
        if (string.IsNullOrEmpty(exePath))
        {
            await ShowInfoAsync("请先指定游戏可执行文件（EXE）。");
            return;
        }
        if (!File.Exists(exePath))
        {
            var confirmDialog = new ContentDialog
            {
                Title = "路径不存在",
                Content = $"所指定的可执行文件不存在：\n{exePath}\n\n是否仍要生成启动命令？",
                PrimaryButtonText = "继续",
                CloseButtonText = "取消",
                XamlRoot = XamlRoot
            };
            if (await confirmDialog.ShowAsync() != ContentDialogResult.Primary) return;
        }

        var command = _steamService.GenerateLaunchCommandFromExe(exePath);

        var dataPackage = new DataPackage();
        dataPackage.SetText(command);
        Clipboard.SetContent(dataPackage);

        _logService.AddLog("[自定义页] 启动命令已复制到剪贴板");
        _logService.AddLog($"[自定义页] 命令：{command}");
    }

    // ── 显式保存 ──────────────────────────────────────────────────────────────

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        PersistCurrentPreset();
        _logService.AddLog("[自定义页] 已保存当前配置");
        await ShowInfoAsync("当前配置已保存。下次打开该页将自动恢复。");
    }

    private void PersistCurrentPreset()
    {
        _settings = _settingsService.Load();
        _settings.CurrentCustomManifest = BuildPresetFromUI();
        if (!_settingsService.Save(_settings))
            _logService.AddLog("[自定义页][警告] 设置保存失败");
    }

    // ── 校验 ──────────────────────────────────────────────────────────────────

    private bool ValidateForGenerate()
    {
        if (string.IsNullOrWhiteSpace(_settings.SteamInstallPath)
            || string.IsNullOrWhiteSpace(_settings.SteamLibraryPath)
            || string.IsNullOrWhiteSpace(_settings.SteamId))
        {
            _logService.AddLog("[自定义页] 错误: Steam 全局配置不完整，请前往设置页配置");
            globalConfigInfoBar.IsOpen = true;
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtAppId.Text))
        {
            _logService.AddLog("[自定义页] 错误: 请填写 AppID");
            return false;
        }
        if (!long.TryParse(txtAppId.Text.Trim(), out _))
        {
            _logService.AddLog("[自定义页] 错误: AppID 必须为数字");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtDepotId.Text))
        {
            _logService.AddLog("[自定义页] 错误: 请填写 DepotID");
            return false;
        }
        if (!long.TryParse(txtDepotId.Text.Trim(), out _))
        {
            _logService.AddLog("[自定义页] 错误: DepotID 必须为数字");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtDisplayName.Text))
        {
            _logService.AddLog("[自定义页] 错误: 请填写显示名称");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtInstallDir.Text))
        {
            _logService.AddLog("[自定义页] 错误: 请填写安装目录名");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtBuildId.Text))
        {
            _logService.AddLog("[自定义页] 错误: 请先获取或填写 BuildID");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtManifest.Text))
        {
            _logService.AddLog("[自定义页] 错误: 请先获取或填写 Manifest");
            return false;
        }
        return true;
    }

    // ── 日志 ──────────────────────────────────────────────────────────────────

    private void ClearLog_Click(object sender, RoutedEventArgs e) => _logService.Clear();

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
