using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WutheringWavesSteamHelper.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "鸣潮 Steam 助手 v1.2.0";

        // 默认导航到鸣潮页面
        ContentFrame.Navigate(typeof(Views.WutheringWavesPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            if (tag == "wuwa")
            {
                ContentFrame.Navigate(typeof(Views.WutheringWavesPage));
            }
        }
    }
}
