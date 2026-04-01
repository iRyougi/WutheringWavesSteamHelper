using Microsoft.UI.Xaml.Controls;
using WutheringWavesSteamHelper.ViewModels;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WutheringWavesSteamHelper.Views;

public sealed partial class WutheringWavesPage : Page
{
    public MainViewModel ViewModel { get; }

    public WutheringWavesPage()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
    }

    private async void BrowseSteam_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        var hwnd = WindowNative.GetWindowHandle(App.Current.m_window);
        InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            ViewModel.SteamInstallPath = folder.Path;
            ViewModel.AddLog($"已选择 Steam 安装路径：{folder.Path}");
        }
    }

    private async void BrowseLibrary_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        var hwnd = WindowNative.GetWindowHandle(App.Current.m_window);
        InitializeWithWindow.Initialize(picker, hwnd);
        picker.FileTypeFilter.Add("*");

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            ViewModel.SteamLibraryPath = folder.Path;
            ViewModel.AddLog($"已选择 SteamLibrary 路径：{folder.Path}");
        }
    }

    private async void GenerateLaunchCommand_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var command = ViewModel.GenerateLaunchCommandText();
        if (command != null)
        {
            var dialog = new ContentDialog
            {
                Title = "Steam 启动命令",
                Content = new TextBox
                {
                    Text = command,
                    IsReadOnly = true,
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                    MinHeight = 100
                },
                CloseButtonText = "关闭",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
        else
        {
            var dialog = new ContentDialog
            {
                Title = "错误",
                Content = "未找到国服鸣潮安装路径",
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    private void OpenLauncher_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.OpenLauncher();
    }
}
