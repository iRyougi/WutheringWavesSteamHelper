using Microsoft.UI.Xaml.Controls;
using WutheringWavesSteamHelper.WinUI.ViewModels;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WutheringWavesSteamHelper.WinUI.Views;

public sealed partial class WutheringWavesPage : Page
{
    public MainViewModel ViewModel { get; }

    public WutheringWavesPage()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
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
}
