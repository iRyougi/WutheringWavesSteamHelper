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

    // 预设列表与守卫（避免 SelectionChanged 事件重入）
    private List<CustomManifestPreset> _presets = new();
    private bool _suppressSelectionChanged;

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
        _settings.EnsureCustomManifestPresets();
        _presets = _settings.CustomManifestPresets;

        // 选中上次记住的预设；找不到则取第一个
        var index = _presets.FindIndex(
            p => string.Equals(p.Name, _settings.CurrentCustomManifestName, StringComparison.OrdinalIgnoreCase));
        if (index < 0) index = 0;

        RefreshPresetComboBox(index);
        LoadPreset(_presets[index]);
        UpdateGlobalConfigInfoBar();
    }

    /// <summary>用 _presets 重填下拉，并选中指定项（带事件守卫）。</summary>
    private void RefreshPresetComboBox(int selectedIndex)
    {
        _suppressSelectionChanged = true;
        cmbPreset.ItemsSource = null;
        cmbPreset.ItemsSource = _presets;
        if (_presets.Count > 0)
            cmbPreset.SelectedIndex = Math.Clamp(selectedIndex, 0, _presets.Count - 1);
        _suppressSelectionChanged = false;
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
        txtExecutableFileName.Text = preset.ExecutableFileName;
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

    /// <summary>从表单构造预设。Name 取当前选中预设的名字（未选中则「默认」）。</summary>
    private CustomManifestPreset BuildPresetFromUI()
    {
        var langTag = (cmbLanguageCode.SelectedItem as ComboBoxItem)?.Tag as string ?? "schinese";
        var current = GetSelectedPreset();
        return new CustomManifestPreset
        {
            Name = current?.Name is { Length: > 0 } name ? name : "默认",
            AppId = txtAppId.Text.Trim(),
            DepotId = txtDepotId.Text.Trim(),
            BuildId = txtBuildId.Text.Trim(),
            Manifest = txtManifest.Text.Trim(),
            GameDisplayName = txtDisplayName.Text.Trim(),
            InstallDir = txtInstallDir.Text.Trim(),
            ClientExePath = txtClientExePath.Text.Trim(),
            ExecutableFileName = txtExecutableFileName.Text.Trim(),
            Language = langTag,
        };
    }

    private CustomManifestPreset? GetSelectedPreset()
    {
        var idx = cmbPreset.SelectedIndex;
        return idx >= 0 && idx < _presets.Count ? _presets[idx] : null;
    }

    /// <summary>判断表单内容是否与给定预设逐字段不一致（忽略 Name）。</summary>
    private bool IsDirtyAgainst(CustomManifestPreset preset)
    {
        var form = BuildPresetFromUI();
        return form.AppId != preset.AppId
            || form.DepotId != preset.DepotId
            || form.BuildId != preset.BuildId
            || form.Manifest != preset.Manifest
            || form.GameDisplayName != preset.GameDisplayName
            || form.InstallDir != preset.InstallDir
            || form.ClientExePath != preset.ClientExePath
            || form.ExecutableFileName != preset.ExecutableFileName
            || form.Language != preset.Language;
    }

    // ── 预设管理 ──────────────────────────────────────────────────────────────

    private int _lastSelectedIndex = -1;

    private async void Preset_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionChanged)
        {
            _lastSelectedIndex = cmbPreset.SelectedIndex;
            return;
        }

        var newIndex = cmbPreset.SelectedIndex;
        if (newIndex < 0 || newIndex >= _presets.Count)
        {
            _lastSelectedIndex = newIndex;
            return;
        }

        // 切换前若上一个预设有未保存改动，弹确认
        if (_lastSelectedIndex >= 0 && _lastSelectedIndex < _presets.Count
            && _lastSelectedIndex != newIndex
            && IsDirtyAgainst(_presets[_lastSelectedIndex]))
        {
            var dialog = new ContentDialog
            {
                Title = "放弃未保存的改动？",
                Content = "当前表单存在未保存的改动，切换预设将丢失这些改动。是否继续？",
                PrimaryButtonText = "放弃改动并切换",
                CloseButtonText = "取消",
                XamlRoot = XamlRoot
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                // 回滚选择
                _suppressSelectionChanged = true;
                cmbPreset.SelectedIndex = _lastSelectedIndex;
                _suppressSelectionChanged = false;
                return;
            }
        }

        _lastSelectedIndex = newIndex;
        LoadPreset(_presets[newIndex]);
    }

    private async void NewPreset_Click(object sender, RoutedEventArgs e)
    {
        var name = await PromptForPresetNameAsync("新建预设", "");
        if (name == null) return;

        var preset = new CustomManifestPreset { Name = name };
        _presets.Add(preset);
        var index = _presets.Count - 1;
        RefreshPresetComboBox(index);
        _lastSelectedIndex = index;
        LoadPreset(preset);
        PersistCurrentPreset();
        _logService.AddLog($"[自定义页] 已新建预设：{name}");
    }

    private async void SaveAsPreset_Click(object sender, RoutedEventArgs e)
    {
        var name = await PromptForPresetNameAsync("另存为新预设", "");
        if (name == null) return;

        var preset = BuildPresetFromUI();
        preset.Name = name;
        _presets.Add(preset);
        var index = _presets.Count - 1;
        RefreshPresetComboBox(index);
        _lastSelectedIndex = index;
        LoadPreset(preset);
        PersistCurrentPreset();
        _logService.AddLog($"[自定义页] 已另存为预设：{name}");
    }

    private async void RenamePreset_Click(object sender, RoutedEventArgs e)
    {
        var current = GetSelectedPreset();
        if (current == null) return;

        var name = await PromptForPresetNameAsync("重命名预设", current.Name, current.Name);
        if (name == null) return;

        current.Name = name;
        var index = cmbPreset.SelectedIndex;
        RefreshPresetComboBox(index);
        _lastSelectedIndex = index;
        PersistCurrentPreset();
        _logService.AddLog($"[自定义页] 预设已重命名为：{name}");
    }

    private async void DeletePreset_Click(object sender, RoutedEventArgs e)
    {
        var current = GetSelectedPreset();
        if (current == null) return;

        var dialog = new ContentDialog
        {
            Title = "删除预设",
            Content = $"确定要删除预设「{current.Name}」吗？此操作不可撤销。",
            PrimaryButtonText = "删除",
            CloseButtonText = "取消",
            XamlRoot = XamlRoot
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

        _presets.Remove(current);
        if (_presets.Count == 0)
            _presets.Add(new CustomManifestPreset { Name = "默认" });

        RefreshPresetComboBox(0);
        _lastSelectedIndex = 0;
        LoadPreset(_presets[0]);
        PersistCurrentPreset();
        _logService.AddLog($"[自定义页] 已删除预设：{current.Name}");
    }

    /// <summary>
    /// 弹出对话框输入预设名，校验非空且不与现有重名（OrdinalIgnoreCase）。
    /// 取消或校验失败返回 null；excludeName 为允许保留的原名（重命名时排除自身）。
    /// </summary>
    private async Task<string?> PromptForPresetNameAsync(string title, string defaultText, string? excludeName = null)
    {
        var textBox = new TextBox { Text = defaultText, PlaceholderText = "请输入预设名称" };
        var dialog = new ContentDialog
        {
            Title = title,
            Content = textBox,
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            XamlRoot = XamlRoot
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return null;

        var name = textBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await ShowInfoAsync("预设名称不能为空。");
            return null;
        }

        var duplicate = _presets.Any(p =>
            !(excludeName != null && string.Equals(p.Name, excludeName, StringComparison.OrdinalIgnoreCase))
            && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (duplicate)
        {
            await ShowInfoAsync($"已存在同名预设「{name}」，请换一个名称。");
            return null;
        }

        return name;
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
        // 重新加载全局字段（Steam 路径等），但保留内存中的预设列表
        var disk = _settingsService.Load();
        _settings.SteamInstallPath = disk.SteamInstallPath;
        _settings.SteamLibraryPath = disk.SteamLibraryPath;
        _settings.SteamId = disk.SteamId;
        UpdateGlobalConfigInfoBar();

        if (!ValidateForGenerate()) return;

        // 解析并校验占位 exe 文件名（在写文件前）
        var displayNameForExe = txtDisplayName.Text.Trim();
        var exeFileName = txtExecutableFileName.Text.Trim();
        if (string.IsNullOrEmpty(exeFileName))
            exeFileName = displayNameForExe + ".exe";
        else if (!exeFileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            exeFileName += ".exe";

        if (exeFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            _logService.AddLog($"[自定义页] 错误: 占位文件名含非法字符：{exeFileName}");
            await ShowInfoAsync($"Steam 占位文件名含非法字符：\n{exeFileName}\n\n请修正后重试。");
            return;
        }

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

            // 创建安装目录 + 占位 EXE（修复 #23）
            var ph = _steamService.EnsureGameDirAndPlaceholder(libraryPath, installDir, exeFileName);
            if (ph.DirCreated)
                _logService.AddLog($"[自定义页] 已创建目录：{ph.GameDirPath}");
            if (ph.ExeCreated)
                _logService.AddLog($"[自定义页] 已创建占位 EXE：{ph.ExePath}");
            else
                _logService.AddLog($"[自定义页] 占位 EXE 已存在，跳过：{ph.ExePath}");

            // 同步保存当前配置（与鸣潮页一致的隐式保存语义）
            PersistCurrentPreset();
            _logService.AddLog("[自定义页][完成] ACF 与占位 EXE 已就绪，请重启 Steam。");

            await ShowInfoAsync($"配置生成成功！\n\n占位 EXE 已就绪：{exeFileName}\n\n请重启 Steam 客户端，然后在库中找到「{displayName}」。");
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

    /// <summary>
    /// 把当前表单写回选中预设，并镜像到 CurrentCustomManifest / CurrentCustomManifestName，落盘。
    /// 仅重载全局字段（Steam 路径等），不重载预设列表，避免覆盖内存中的预设改动。
    /// </summary>
    private void PersistCurrentPreset()
    {
        var index = cmbPreset.SelectedIndex;
        var built = BuildPresetFromUI();

        if (index >= 0 && index < _presets.Count)
            _presets[index] = built;
        else if (_presets.Count == 0)
            _presets.Add(built);

        // 重载磁盘设置以拿到最新全局字段，再覆盖预设相关字段
        var disk = _settingsService.Load();
        disk.CustomManifestPresets = _presets;
        disk.CurrentCustomManifest = built;
        disk.CurrentCustomManifestName = built.Name;
        _settings = disk;

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
