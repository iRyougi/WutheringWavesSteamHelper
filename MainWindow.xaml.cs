using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using WetheringWavesSteamHelper_WinUI.Services;

namespace WetheringWavesSteamHelper_WinUI;

public sealed partial class MainWindow : Window
{
    private string _forceDownloadUrl = "";

    public MainWindow()
    {
        InitializeComponent();
        ConfigureFixedWindow();

        txtTitleBar.Text = AppInfo.AppName;

        // 订阅更新通知（主窗口负责全局展示）
        UpdateService.Instance.UpdateAvailable += OnUpdateAvailable;

        // 默认选中第一项（鸣潮）
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(Views.Pages.WutheringWavesPage));
    }

    // ── 更新通知处理 ──────────────────────────────────────────────────────────

    private void OnUpdateAvailable(string message, string downloadUrl, bool forceUpdate)
    {
        _forceDownloadUrl = downloadUrl;

        DispatcherQueue.TryEnqueue(() =>
        {
            if (forceUpdate)
            {
                // 强制更新：显示全屏遮罩 + 顶部横幅，锁定所有导航
                txtForceUpdateMsg.Text = string.IsNullOrWhiteSpace(message)
                    ? "当前版本存在严重问题，必须更新后才能继续使用。"
                    : message;
                forceUpdateBanner.IsOpen = true;
                forceUpdateOverlay.Visibility = Visibility.Visible;
                NavView.IsEnabled = false;
            }
            else
            {
                // 普通更新：右上角显示提示按钮
                btnUpdateBadge.Visibility = Visibility.Visible;
            }
        });
    }

    // ── 按钮事件 ──────────────────────────────────────────────────────────────

    private void BtnUpdateBadge_Click(object sender, RoutedEventArgs e)
    {
        // 跳转到设置页
        NavView.SelectedItem = NavView.FooterMenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(i => i.Tag?.ToString() == "Settings");
        ContentFrame.Navigate(typeof(Views.Pages.SettingsPage));
    }

    private async void BtnForceUpdateDownload_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_forceDownloadUrl)) return;
        try
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(_forceDownloadUrl));
        }
        catch { }
    }

    // ── 窗口配置 ──────────────────────────────────────────────────────────────

    private void ConfigureFixedWindow()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.Title = AppInfo.WindowTitle;

        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Icons", "WutheringWavesSteamHelper.ico");
        if (File.Exists(iconPath))
        {
            appWindow.SetIcon(iconPath);
        }

        appWindow.Resize(new SizeInt32(1100, 780));

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;

        var tag = item.Tag?.ToString();
        switch (tag)
        {
            case "WutheringWaves":
                ContentFrame.Navigate(typeof(Views.Pages.WutheringWavesPage));
                break;
            case "Settings":
                ContentFrame.Navigate(typeof(Views.Pages.SettingsPage));
                break;
            case "Placeholder":
                break;
        }
    }
}
