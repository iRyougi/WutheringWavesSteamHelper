using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Specialized;
using Windows.Storage.Pickers;
using WetheringWavesSteamHelper_WinUI.Models;
using WetheringWavesSteamHelper_WinUI.Services;
using WetheringWavesSteamHelper_WinUI.Services.LauncherCache;

namespace WetheringWavesSteamHelper_WinUI.Views.Pages;

/// <summary>
/// 画质与启动器页（v2.4.0）：查看各画质档位的游戏目录与安装状态，改写启动器缓存，备份恢复，创建目录联接。
/// 文件格式参考 B1ngFeng/wuwa-launcher-path 的 docs/format.md，代码为独立实现。
/// </summary>
public sealed partial class QualityLauncherPage : Page
{
    private readonly LauncherCacheService _service = new();
    private readonly SteamService _steamService = new();
    private readonly SettingsService _settingsService = new();
    private readonly LogService _logService = LogService.Instance;
    private AppSettings _settings = new();
    private LauncherStatus? _status;
    private bool _suppressCandidateChange;
    private NotifyCollectionChangedEventHandler? _logScrollHandler;

    public QualityLauncherPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        logList.ItemsSource = _logService.Logs;
        _logScrollHandler = (_, _) => logScrollViewer.ChangeView(null, logScrollViewer.ScrollableHeight, null);
        _logService.Logs.CollectionChanged += _logScrollHandler;
        _settings = _settingsService.Load();
        await RunSafeAsync("加载", () => RefreshAsync(null));
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_logScrollHandler != null)
            _logService.Logs.CollectionChanged -= _logScrollHandler;
    }

    // ===== 状态读取与渲染 =====

    private async Task RefreshAsync(string? preferredDir)
    {
        btnRedetect.IsEnabled = false;
        try
        {
            var savedDir = _settings.LauncherDirPath;
            var preferred = preferredDir ?? _status?.LauncherDir;
            _status = await Task.Run(() =>
                _service.GetStatus(savedDir, _steamService.DetectCnWutheringWavesPaths(), preferred));
            Render(_status);
        }
        catch (Exception ex)
        {
            _logService.AddLog($"[画质与启动器] 检测失败：{ex.GetType().Name} - {ex.Message}");
            ShowInfo(InfoBarSeverity.Error, "检测失败", ex.Message);
        }
        finally
        {
            btnRedetect.IsEnabled = true;
        }
    }

    private void Render(LauncherStatus status)
    {
        txtLauncherDir.Text = status.LauncherDir is null ? "未找到启动器" : $"启动器目录：{status.LauncherDir}";
        btnOpenLauncherDir.IsEnabled = status.LauncherDir is not null;

        _suppressCandidateChange = true;
        cmbCandidates.ItemsSource = status.CandidateLauncherDirs;
        cmbCandidates.SelectedItem = status.LauncherDir;
        cmbCandidates.Visibility = status.CandidateLauncherDirs.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        _suppressCandidateChange = false;

        var canWrite = status.CanWrite && status.LauncherDir is not null;
        tierList.ItemsSource = status.Bundles.Select(b => new QualityTierRow(b, canWrite)).ToList();
        btnSetAll.IsEnabled = canWrite;
        btnBackup.IsEnabled = status.LauncherDir is not null;
        btnRestore.IsEnabled = status.LauncherDir is not null;

        if (status.LauncherDir is null)
            ShowInfo(InfoBarSeverity.Informational, "未找到新版启动器", "需鸣潮 3.7 及以上的官方启动器。可点「手动选择启动器目录」指定（目录内应含 kr_game_cache 文件夹）。");
        else if (status.Error is not null)
            ShowInfo(InfoBarSeverity.Error, "只读模式", status.Error);
        else if (status.IsLauncherRunning)
            ShowInfo(InfoBarSeverity.Warning, "启动器正在运行", "修改前请完全退出启动器（包括右下角托盘图标），然后点「重新检测」。");
        else
            infoStatus.IsOpen = false;

        _logService.AddLog(status.LauncherDir is null
            ? "[画质与启动器] 未找到新版启动器"
            : $"[画质与启动器] 启动器目录：{status.LauncherDir}；" +
              string.Join("；", status.Bundles.Select(b => $"{b.Tier.BundleName()}={b.InstallDirPath ?? "未配置"}")));
    }

    private void ShowInfo(InfoBarSeverity severity, string title, string message)
    {
        infoStatus.Severity = severity;
        infoStatus.Title = title;
        infoStatus.Message = message;
        infoStatus.IsOpen = true;
    }

    // ===== 启动器目录 =====

    private async void Redetect_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync("重新检测", () => RefreshAsync(null));

    private async void Candidates_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressCandidateChange || cmbCandidates.SelectedItem is not string dir) return;
        await RunSafeAsync("切换启动器目录", () => RefreshAsync(dir));
    }

    private async void BrowseLauncherDir_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync("手动选择启动器目录", BrowseLauncherDirAsync);

    private async Task BrowseLauncherDirAsync()
    {
        var folder = await PickFolderSafelyAsync("画质页-手动选择启动器目录");
        if (folder == null) return;

        if (!LauncherCacheFiles.IsLauncherDir(folder.Path))
        {
            await ShowMessageAsync("所选目录不是新版启动器目录",
                $"未在该目录下找到 {LauncherCacheFiles.CacheDirName}\\{LauncherCacheFiles.GameTempFileName}。\n\n请选择官方启动器的安装目录（例如 D:\\Wuthering Waves）。");
            return;
        }

        _settings.LauncherDirPath = folder.Path;
        if (!_settingsService.Save(_settings))
            _logService.AddLog("[警告] 启动器目录保存失败");
        _logService.AddLog($"已手动指定启动器目录：{folder.Path}");
        await RefreshAsync(folder.Path);
    }

    private void OpenLauncherDir_Click(object sender, RoutedEventArgs e)
    {
        if (_status?.LauncherDir is { } dir) OpenInExplorer(dir);
    }

    // ===== 改写档位路径 =====

    private async void EditTier_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: QualityTier tier })
            await RunSafeAsync($"修改{tier.BundleName()}目录", () => EditTierAsync(tier));
    }

    private async Task EditTierAsync(QualityTier tier)
    {
        if (_status?.LauncherDir is not { } launcherDir) return;

        var oldPath = _status.Bundles.FirstOrDefault(b => b.Tier == tier)?.InstallDirPath ?? "未配置";
        var folder = await PickFolderSafelyAsync($"画质页-修改{tier.BundleName()}目录");
        if (folder == null) return;

        await ApplyAsync(folder.Path,
            $"修改 {tier.DisplayName()} {tier.BundleName()} 的游戏目录",
            $"旧：{oldPath}\n新：{folder.Path}",
            allowMissing => _service.SetBundlePath(launcherDir, tier, folder.Path, allowMissing));
    }

    private async void SetAll_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync("三档统一指向", SetAllAsync);

    private async Task SetAllAsync()
    {
        if (_status?.LauncherDir is not { } launcherDir) return;

        var folder = await PickFolderSafelyAsync("画质页-三档统一指向");
        if (folder == null) return;

        var olds = string.Join("\n", _status.Bundles.Select(b => $"{b.Tier.BundleName()}：{b.InstallDirPath ?? "未配置"}"));
        await ApplyAsync(folder.Path,
            "三档统一指向同一目录",
            $"当前：\n{olds}\n\n全部改为：{folder.Path}",
            allowMissing => _service.SetAllBundlePaths(launcherDir, folder.Path, allowMissing));
    }

    /// <summary>确认 → （必要时）确认缺少客户端 → 后台写入 → 展示结果 → 刷新。</summary>
    private async Task ApplyAsync(string newPath, string title, string detail, Func<bool, WriteResult> write)
    {
        if (!await ConfirmAsync(title, $"{detail}\n\n写入前会自动备份。请确认启动器已完全退出。", "写入"))
            return;

        var allowMissing = false;
        if (!File.Exists(LauncherCacheFiles.ClientExePath(newPath)))
        {
            allowMissing = await ConfirmAsync("未找到游戏客户端",
                $"所选目录中没有 {LauncherCacheFiles.ClientExePath(newPath)}。\n\n启动器可能会要求重新下载或修复。仍然写入吗？", "仍然写入");
            if (!allowMissing) return;
        }

        var result = await Task.Run(() => write(allowMissing));
        await ReportAsync(result);
        await RefreshAsync(null);
    }

    // ===== 备份与恢复 =====

    private async void Backup_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync("立即备份", async () =>
        {
            if (_status?.LauncherDir is not { } launcherDir) return;
            await ReportAsync(await Task.Run(() => _service.Backup(launcherDir)));
        });

    private async void Restore_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync("从备份恢复", RestoreAsync);

    private async Task RestoreAsync()
    {
        if (_status?.LauncherDir is not { } launcherDir) return;

        var backups = _service.ListBackups();
        if (backups.Count == 0)
        {
            await ShowMessageAsync("没有备份", $"备份目录中还没有备份文件：\n{_service.BackupDir}");
            return;
        }

        var combo = new ComboBox
        {
            ItemsSource = backups.Select(b => $"{b.CreatedAt:yyyy-MM-dd HH:mm:ss}  ({b.SizeBytes} 字节)  {Path.GetFileName(b.FilePath)}").ToList(),
            SelectedIndex = 0,
            MinWidth = 520,
        };
        var dialog = new ContentDialog
        {
            Title = "选择要恢复的备份",
            Content = combo,
            PrimaryButtonText = "恢复",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary || combo.SelectedIndex < 0) return;

        var file = backups[combo.SelectedIndex].FilePath;
        await ReportAsync(await Task.Run(() => _service.Restore(launcherDir, file)));
        await RefreshAsync(null);
    }

    private void OpenBackupDir_Click(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(_service.BackupDir);
        OpenInExplorer(_service.BackupDir);
    }

    // ===== 目录联接 =====

    private async void CreateJunction_Click(object sender, RoutedEventArgs e) =>
        await RunSafeAsync("创建目录联接", CreateJunctionAsync);

    private async Task CreateJunctionAsync()
    {
        var folder = await PickFolderSafelyAsync("画质页-选择真实游戏目录");
        if (folder == null) return;

        var error = JunctionService.Validate(folder.Path);
        if (error is not null)
        {
            await ShowMessageAsync("无法创建联接", error);
            return;
        }

        var link = JunctionService.GetLinkPath(folder.Path)!;
        if (!await ConfirmAsync("创建目录联接", $"将创建：\n{link}\n→ 指向 {folder.Path}\n\n联接不占用额外空间，删除联接不会删除真实文件。", "创建"))
            return;

        var result = await Task.Run(() => JunctionService.Create(folder.Path));
        _logService.AddLog($"[目录联接] {result.Message}");
        if (!result.Success)
        {
            await ShowMessageAsync("创建失败", result.Message);
            return;
        }

        var parent = Path.GetDirectoryName(link);
        ShowInfo(InfoBarSeverity.Success, "联接已创建", $"在官方启动器的「定位游戏」中选择：{parent}");

        if (_status is { LauncherDir: { } launcherDir, CanWrite: true }
            && await ConfirmAsync("同时写入启动器缓存？", $"是否把 UHD / HD / SD 三档游戏目录都指向：\n{link}", "写入"))
        {
            await ReportAsync(await Task.Run(() => _service.SetAllBundlePaths(launcherDir, link, allowMissingExe: false)));
            await RefreshAsync(null);
        }
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e) => _logService.Clear();

    // ===== 通用辅助 =====

    /// <summary>async void 事件处理器的统一兜底：异常写日志并提示，不外抛到全局处理器。</summary>
    private async Task RunSafeAsync(string context, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _logService.AddLog($"[画质与启动器-{context}] 失败：{ex.GetType().Name} - {ex.Message}");
            try
            {
                await ShowMessageAsync("操作失败", $"{context}失败：{ex.Message}");
            }
            catch (Exception dialogEx)
            {
                _logService.AddLog($"[画质与启动器-{context}] 显示错误提示也失败了：{dialogEx.Message}");
            }
        }
    }

    private async Task ReportAsync(WriteResult result)
    {
        foreach (var message in result.Messages)
            _logService.AddLog($"[画质与启动器] {message}");

        if (result.Success)
            ShowInfo(InfoBarSeverity.Success, "操作成功", string.Join("\n", result.Messages));
        else
            await ShowMessageAsync("操作失败", string.Join("\n", result.Messages));
    }

    private void OpenInExplorer(string dir)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{dir}\"",
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            _logService.AddLog($"打开目录失败：{ex.Message}");
        }
    }

    private async Task<bool> ConfirmAsync(string title, string content, string primaryText)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = new TextBlock { Text = content, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true },
            PrimaryButtonText = primaryText,
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private async Task ShowMessageAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = new TextBlock { Text = content, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true },
            CloseButtonText = "确定",
            XamlRoot = XamlRoot,
        };
        await dialog.ShowAsync();
    }

    private async Task<Windows.Storage.StorageFolder?> PickFolderSafelyAsync(string context)
    {
        try
        {
            var picker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
            picker.FileTypeFilter.Add("*");
            return await picker.PickSingleFolderAsync();
        }
        catch (Exception ex)
        {
            _logService.AddLog($"[{context}] 文件夹选择框打开失败：{ex.GetType().Name} - {ex.Message}");
            await ShowMessageAsync("提示", $"无法打开文件夹选择框，这通常是系统 Shell 组件异常导致的。\n\n错误信息：{ex.Message}");
            return null;
        }
    }
}
