using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Specialized;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.DataTransfer;
using WetheringWavesSteamHelper_WinUI.Services;
using WetheringWavesSteamHelper_WinUI.Models;

namespace WetheringWavesSteamHelper_WinUI.Views.Pages;

public sealed partial class WutheringWavesPage : Page
{
    private readonly SteamService _steamService = new();
    private readonly SettingsService _settingsService = new();
    private readonly LogService _logService = LogService.Instance;
    private AppSettings _settings = new();

    // 启动初始化期间禁止触发游戏源检测
    private bool _sourceChanging = true;

    // 是否已在本次页面生命周期内提示过手动输入路径的风险
    private bool _clientExePathWarningShown = false;

    // 日志 CollectionChanged handler，用于在 Unloaded 时取消订阅
    private NotifyCollectionChangedEventHandler? _logScrollHandler;

    public WutheringWavesPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        logList.ItemsSource = _logService.Logs;
        // 日志新增时自动滚动到底部
        _logScrollHandler = (_, _) =>
        {
            logScrollViewer.ChangeView(null, logScrollViewer.ScrollableHeight, null);
        };
        _logService.Logs.CollectionChanged += _logScrollHandler;
        _settings = _settingsService.Load();
        LoadSettings();

        // 启动时根据保存设置检测游戏源，若检测不到则提示
        if (_settings.CnGameSource == "wegame")
        {
            rdoWeGame.IsChecked = true;
            if (_steamService.DetectWeGameInstallPath() == null)
                _logService.AddLog("提示：上次使用 WeGame 模式，但未检测到 WeGame 鸣潮安装");
        }
        else
        {
            var hasOfficial = _steamService.DetectCnWutheringWavesPaths()
                .Any(p => File.Exists(Path.Combine(p, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe")));
            if (!hasOfficial && _steamService.DetectWeGameInstallPath() != null)
            {
                rdoWeGame.IsChecked = true;
                _logService.AddLog("已自动切换到 WeGame 模式（未检测到官方启动器版本）");
            }
        }

        // 启动完成后允许用户切换时触发检测
        _sourceChanging = false;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_logScrollHandler != null)
            _logService.Logs.CollectionChanged -= _logScrollHandler;
    }

    private void LoadSettings()
    {
        // 重新加载设置（用户可能在设置页修改了全局配置）
        _settings = _settingsService.Load();

        txtBuildId.Text = _settings.BuildId;
        txtManifest.Text = _settings.Manifest;

        if (_settings.CnGameSource == "wegame")
            rdoWeGame.IsChecked = true;
        else
            rdoOfficial.IsChecked = true;
    }

    private async void GameSource_Checked(object sender, RoutedEventArgs e)
    {
        // 初始化阶段不触发检测
        if (_sourceChanging) return;

        if (rdoWeGame.IsChecked == true)
        {
            // 切换到 WeGame 前先弹出确认提示
            var confirmDialog = new ContentDialog
            {
                Title = "切换到 WeGame 版本",
                Content = "WeGame版本需要使用官方启动器修复才可使用！是否继续您的操作？\n\n详情请访问 设置 - 帮助文档",
                PrimaryButtonText = "继续",
                CloseButtonText = "取消",        // CloseButton 为默认高亮按钮
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                // 用户取消 → 回退到官方启动器
                _sourceChanging = true;
                rdoOfficial.IsChecked = true;
                _sourceChanging = false;
                return;
            }

            // 用户选择继续 → 检测 WeGame 安装
            var wegamePath = _steamService.DetectWeGameInstallPath();
            if (wegamePath != null)
            {
                _logService.AddLog($"已检测到 WeGame 鸣潮路径：{wegamePath}");
            }
            else
            {
                _logService.AddLog("未检测到 WeGame 鸣潮安装，已自动切回官方启动器模式");
                _sourceChanging = true;
                rdoOfficial.IsChecked = true;
                _sourceChanging = false;
            }
        }
        else if (rdoOfficial.IsChecked == true)
        {
            var hasOfficial = _steamService.DetectCnWutheringWavesPaths()
                .Any(p => File.Exists(Path.Combine(p, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe")));
            if (!hasOfficial)
            {
                _logService.AddLog("未检测到官方启动器版鸣潮安装，已自动切回 WeGame 模式");
                _sourceChanging = true;
                rdoWeGame.IsChecked = true;
                _sourceChanging = false;
            }
        }
    }

    private async void BrowseClientExe_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add(".exe");
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

        var file = await PickFileSafelyAsync(picker, "鸣潮页-手动选择客户端");
        if (file == null) return;

        // 必须选择 .exe 文件（picker 已过滤，此处双重确认）
        if (!file.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            await ShowInfoAsync("所选文件不是 .exe 文件，请重新选择可执行文件。");
            return;
        }

        // 建议客户端文件名为 Client-Win64-Shipping.exe，若不符则提示确认
        if (!file.Name.Equals("Client-Win64-Shipping.exe", StringComparison.OrdinalIgnoreCase))
        {
            var confirmDialog = new ContentDialog
            {
                Title = "确认文件",
                Content = $"所选文件不是标准客户端文件（Client-Win64-Shipping.exe），是否仍然使用？\n\n所选文件：{file.Name}\n路径：{file.Path}",
                PrimaryButtonText = "使用",
                CloseButtonText = "重新选择",
                XamlRoot = XamlRoot
            };
            if (await confirmDialog.ShowAsync() != ContentDialogResult.Primary) return;
        }

        txtClientExePath.Text = file.Path;
        _logService.AddLog($"已手动指定客户端路径：{file.Path}");
    }

    private void ClearClientExe_Click(object sender, RoutedEventArgs e)
    {
        txtClientExePath.Text = string.Empty;
        _logService.AddLog("已清除手动指定的客户端路径，将使用自动检测");
    }

    private async void FetchFromSteamDB_Click(object sender, RoutedEventArgs e)
    {
        // 防止重复点击
        btnFetchSteamDB.IsEnabled = false;
        var originalContent = btnFetchSteamDB.Content;
        btnFetchSteamDB.Content = "正在获取...";

        _logService.AddLog("正在从 SteamDB 获取 BuildID 和 Manifest...");

        try
        {
            var result = await _steamService.FetchSteamDbInfoAsync();

            if (result.HasValue)
            {
                txtBuildId.Text = result.Value.buildId;
                txtManifest.Text = result.Value.manifest;
                _logService.AddLog($"获取成功 - BuildID: {result.Value.buildId}, Manifest: {result.Value.manifest}");
            }
            else
            {
                _logService.AddLog("获取失败，请检查网络连接或手动填写");
                await ShowInfoAsync("无法从 SteamDB 获取信息，请检查网络连接。\n\n也可以手动访问 https://steamdb.info/app/3513350/depots/ 获取信息后填写。");
            }
        }
        catch (Exception ex)
        {
            _logService.AddLog($"获取异常：{ex.Message}");
        }
        finally
        {
            btnFetchSteamDB.IsEnabled = true;
            btnFetchSteamDB.Content = originalContent;
        }
    }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInputs()) return;

        // 防止重复点击
        btnGenerate.IsEnabled = false;

        try
        {
            var libraryPath = _settings.SteamLibraryPath.Trim();
            var steamInstallPath = _settings.SteamInstallPath.Trim();
            var steamappsPath = Path.Combine(libraryPath, "steamapps");
            var commonPath = Path.Combine(steamappsPath, "common");
            var gamePath = Path.Combine(commonPath, "Wuthering Waves");

            // ACF 文件中的 LauncherPath 应指向 steam.exe
            var launcherPath = Path.Combine(steamInstallPath, "steam.exe");

            var acfPath = Path.Combine(steamappsPath, "appmanifest_3513350.acf");
            var exePath = Path.Combine(gamePath, "Wuthering Waves.exe");

            // 若 ACF 文件已存在，询问是否覆盖
            if (File.Exists(acfPath))
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "文件已存在",
                    Content = "检测到配置文件已存在：\nappmanifest_3513350.acf\n\n是否要覆盖该文件？",
                    PrimaryButtonText = "覆盖",
                    CloseButtonText = "取消",
                    XamlRoot = XamlRoot
                };
                var confirmResult = await confirmDialog.ShowAsync();
                if (confirmResult != ContentDialogResult.Primary)
                {
                    _logService.AddLog("用户取消覆盖 ACF 文件，操作已跳过。");
                    return;
                }
                _logService.AddLog("用户选择覆盖 ACF 文件。");
            }

            var acfContent = _steamService.GenerateAcfContent(
                launcherPath,
                txtBuildId.Text.Trim(),
                _settings.SteamId.Trim(),
                txtManifest.Text.Trim()
            );

            if (!Directory.Exists(steamappsPath))
            {
                Directory.CreateDirectory(steamappsPath);
                _logService.AddLog($"已创建目录：{steamappsPath}");
            }

            await File.WriteAllTextAsync(acfPath, acfContent);
            _logService.AddLog($"已生成：{acfPath}");

            if (!Directory.Exists(gamePath))
            {
                Directory.CreateDirectory(gamePath);
                _logService.AddLog($"已创建目录：{gamePath}");
            }

            // 仅在占位 EXE 不存在时创建（不覆盖真实 EXE）
            if (!File.Exists(exePath))
            {
                File.Create(exePath).Dispose();
                _logService.AddLog($"已创建占位 EXE：{exePath}");
            }
            else
            {
                _logService.AddLog($"EXE 文件已存在，跳过：{exePath}");
            }

            SaveSettings();
            _logService.AddLog("[完成] 全部操作已完成，请重启 Steam。");

            await ShowInfoAsync("配置生成成功！\n\n请重启 Steam 客户端，然后在库中找到「Wuthering Waves」并启动。");
        }
        catch (Exception ex)
        {
            _logService.AddLog($"[错误] 操作失败：{ex.Message}");
            await ShowInfoAsync($"生成过程中出现错误：\n{ex.Message}");
        }
        finally
        {
            btnGenerate.IsEnabled = true;
        }
    }

    private async void CopyCommand_Click(object sender, RoutedEventArgs e)
    {
        _logService.AddLog("正在搜索国服鸣潮安装路径...");

        string? selectedPath = null;

        // 如果用户已手动指定客户端 exe，优先使用
        var manualClientExePath = txtClientExePath.Text.Trim();
        if (!string.IsNullOrEmpty(manualClientExePath) && File.Exists(manualClientExePath))
        {
            // 从 exe 路径推导出游戏根目录
            // 官方：...\Wuthering Waves Game\Client\Binaries\Win64\Client-Win64-Shipping.exe → ...\Wuthering Waves Game 的上级
            // WeGame：...\Client\Binaries\Win64\Client-Win64-Shipping.exe → Client 的上级
            var exeDir = Path.GetDirectoryName(manualClientExePath)!;   // Win64
            var binariesDir = Path.GetDirectoryName(exeDir)!;           // Binaries
            var clientDir = Path.GetDirectoryName(binariesDir)!;        // Client
            var gameRootDir = Path.GetDirectoryName(clientDir)!;        // 游戏根目录

            // 判断是否官方结构（含 Wuthering Waves Game 层）
            if (Path.GetFileName(gameRootDir).Equals("Wuthering Waves Game", StringComparison.OrdinalIgnoreCase))
                selectedPath = Path.GetDirectoryName(gameRootDir)!;
            else
                selectedPath = gameRootDir;

            _logService.AddLog($"使用手动指定的客户端路径，推导安装目录：{selectedPath}");
        }
        else if (rdoWeGame.IsChecked == true)
        {
            // WeGame 模式：优先读注册表
            selectedPath = _steamService.DetectWeGameInstallPath();
            if (selectedPath == null)
            {
                _logService.AddLog("未检测到 WeGame 鸣潮，请手动选择");
                var picker = new FolderPicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                picker.FileTypeFilter.Add("*");
                var folder = await PickFolderSafelyAsync(picker, "鸣潮页-手动选择WeGame安装目录");
                if (folder == null) return;

                var exePath = Path.Combine(folder.Path, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
                if (!File.Exists(exePath))
                {
                    _logService.AddLog($"所选路径中未找到客户端文件，请确认选择了 WeGame 鸣潮的安装根目录（含 Client 文件夹）");
                    await ShowInfoAsync($"所选路径中未找到客户端文件：\n{exePath}\n\n请确认选择了 WeGame 鸣潮的安装根目录（含 Client 文件夹）。");
                    return;
                }
                selectedPath = folder.Path;
            }
        }
        else
        {
            // 官方启动器模式
            var paths = _steamService.DetectCnWutheringWavesPaths()
                .Where(p => File.Exists(Path.Combine(p, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe")))
                .ToList();

            if (paths.Count == 0)
            {
                _logService.AddLog("未找到官方启动器鸣潮路径，请手动选择");
                var picker = new FolderPicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                picker.FileTypeFilter.Add("*");
                var folder = await PickFolderSafelyAsync(picker, "鸣潮页-手动选择官服安装目录");
                if (folder == null) return;

                var exePath = Path.Combine(folder.Path, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
                if (!File.Exists(exePath))
                {
                    _logService.AddLog("所选路径中未找到客户端文件，请确认选择了国服鸣潮的安装根目录");
                    await ShowInfoAsync($"所选路径中未找到客户端文件：\n{exePath}\n\n请确认选择了国服鸣潮的安装根目录。");
                    return;
                }
                selectedPath = folder.Path;
            }
            else if (paths.Count == 1)
            {
                selectedPath = paths[0];
            }
            else
            {
                // 多个路径：让用户选择
                var comboBox = new ComboBox
                {
                    ItemsSource = paths,
                    SelectedIndex = 0,
                    MinWidth = 520
                };
                var dialog = new ContentDialog
                {
                    Title = "选择国服鸣潮安装路径",
                    Content = comboBox,
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                    XamlRoot = XamlRoot
                };
                var result = await dialog.ShowAsync();
                selectedPath = (result == ContentDialogResult.Primary && comboBox.SelectedItem is string sel)
                    ? sel : paths[0];
            }
        }

        _logService.AddLog($"已找到鸣潮路径：{selectedPath}");
        var command = _steamService.GenerateLaunchCommand(selectedPath);

        var dataPackage = new DataPackage();
        dataPackage.SetText(command);
        Clipboard.SetContent(dataPackage);

        SaveSettings();
        _logService.AddLog("启动命令已复制到剪贴板");
        _logService.AddLog($"命令：{command}");
    }

    private async void OpenLauncher_Click(object sender, RoutedEventArgs e)
    {
        // WeGame 模式下无官方启动器
        if (rdoWeGame.IsChecked == true)
        {
            await ShowInfoAsync("WeGame 版鸣潮请通过 WeGame 客户端启动游戏。\n\n此按钮仅适用于官方启动器版本。");
            return;
        }

        _logService.AddLog("正在搜索官方启动器...");

        string? launcherExe = null;

        // 先在已知安装路径中搜索 launcher.exe
        var paths = _steamService.DetectCnWutheringWavesPaths();
        foreach (var installPath in paths)
        {
            var candidate = Path.Combine(installPath, "launcher.exe");
            if (File.Exists(candidate))
            {
                launcherExe = candidate;
                break;
            }
        }

        // 未找到则让用户手动选择
        if (launcherExe == null)
        {
            _logService.AddLog("未自动找到官方启动器，请手动选择");

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            picker.FileTypeFilter.Add(".exe");
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

            var file = await PickFileSafelyAsync(picker, "鸣潮页-手动选择官方启动器");
            if (file == null) return;

            if (!file.Name.Equals("launcher.exe", StringComparison.OrdinalIgnoreCase))
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "提示",
                    Content = $"所选文件不是 launcher.exe，是否仍然使用？\n{file.Path}",
                    PrimaryButtonText = "使用",
                    CloseButtonText = "取消",
                    XamlRoot = XamlRoot
                };
                if (await confirmDialog.ShowAsync() != ContentDialogResult.Primary) return;
            }

            launcherExe = file.Path;
        }

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = launcherExe,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(launcherExe)
            };
            System.Diagnostics.Process.Start(psi);
            _logService.AddLog($"已启动官方启动器：{launcherExe}");
        }
        catch (Exception ex)
        {
            _logService.AddLog($"启动失败：{ex.Message}");
            await ShowInfoAsync($"无法启动官方启动器：\n{ex.Message}");
        }
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e)
    {
        _logService.Clear();
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(_settings.SteamInstallPath))
        {
            _logService.AddLog("错误: 请先在「设置」中配置 Steam 安装路径");
            return false;
        }
        if (string.IsNullOrWhiteSpace(_settings.SteamLibraryPath))
        {
            _logService.AddLog("错误: 请先在「设置」中配置 SteamLibrary 路径");
            return false;
        }
        if (string.IsNullOrWhiteSpace(_settings.SteamId))
        {
            _logService.AddLog("错误: 请先在「设置」中配置 Steam ID（SteamID64）");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtBuildId.Text))
        {
            _logService.AddLog("错误: 请先获取 BuildID");
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtManifest.Text))
        {
            _logService.AddLog("错误: 请先获取 Manifest ID");
            return false;
        }
        return true;
    }

    private void SaveSettings()
    {
        // 鸣潮页只保存鸣潮专属字段；全局字段由设置页负责保存
        _settings.BuildId = txtBuildId.Text.Trim();
        _settings.Manifest = txtManifest.Text.Trim();
        _settings.CnGameSource = rdoWeGame.IsChecked == true ? "wegame" : "official";

        if (!_settingsService.Save(_settings))
            _logService.AddLog("[警告] 设置保存失败，下次启动将无法自动填入");
    }

    private async void ClientExePath_GotFocus(object sender, RoutedEventArgs e)
    {
        if (_clientExePathWarningShown) return;
        _clientExePathWarningShown = true;

        var dialog = new ContentDialog
        {
            Title = "手动输入路径",
            Content = "请确保填写的是正确的游戏可执行文件路径。路径错误会导致游戏无法启动；如果填的是其他程序的路径，Steam 启动时会执行那个程序而不是游戏。建议优先使用「手动选择」按钮选择文件。",
            CloseButtonText = "知道了",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async Task<Windows.Storage.StorageFile?> PickFileSafelyAsync(Windows.Storage.Pickers.FileOpenPicker picker, string context)
    {
        try
        {
            return await picker.PickSingleFileAsync();
        }
        catch (Exception ex)
        {
            _logService.AddLog($"[{context}] 文件选择框打开失败：{ex.GetType().Name} - {ex.Message}");
            try
            {
                await ShowInfoAsync($"无法打开文件选择框，这通常是系统 Shell 组件异常导致的。\n\n错误信息：{ex.Message}\n\n您可以尝试直接在路径输入框中粘贴文件路径。");
            }
            catch (Exception dialogEx)
            {
                _logService.AddLog($"[{context}] 显示错误提示弹窗也失败了：{dialogEx.GetType().Name} - {dialogEx.Message}");
            }
            return null;
        }
    }

    private async Task<Windows.Storage.StorageFolder?> PickFolderSafelyAsync(FolderPicker picker, string context)
    {
        try
        {
            return await picker.PickSingleFolderAsync();
        }
        catch (Exception ex)
        {
            _logService.AddLog($"[{context}] 文件夹选择框打开失败：{ex.GetType().Name} - {ex.Message}");
            try
            {
                await ShowInfoAsync($"无法打开文件夹选择框，这通常是系统 Shell 组件异常导致的。\n\n错误信息：{ex.Message}");
            }
            catch (Exception dialogEx)
            {
                _logService.AddLog($"[{context}] 显示错误提示弹窗也失败了：{dialogEx.GetType().Name} - {dialogEx.Message}");
            }
            return null;
        }
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
