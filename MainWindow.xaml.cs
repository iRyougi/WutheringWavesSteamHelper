using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace WetheringWavesSteamHelper_WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ConfigureFixedWindow();

        txtTitleBar.Text = AppInfo.AppName;

        // 默认选中第一项（鸣潮）
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(Views.Pages.WutheringWavesPage));
    }

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
                // 恢复选中上一个有效项，不做导航
                break;
        }
    }
}
