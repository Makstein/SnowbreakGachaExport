using System.Collections.ObjectModel;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = "SnowbreakToolbox";

    [ObservableProperty]
    private ObservableCollection<object> _menuItems =
    [
        new NavigationViewItem()
        {
            Content = "启动",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Home24, FontWeight = FontWeights.SemiBold },
            TargetPageType = typeof(Views.Pages.DashboardPage)
        },
        new NavigationViewItem()
        {
            Content = "记录统计",
            Icon = new SymbolIcon { Symbol = SymbolRegular.DocumentOnePage20, FontWeight = FontWeights.SemiBold },
            TargetPageType = typeof(Views.Pages.GachaHistoryPage),
            FontFamily = new FontFamily("Microsoft YaHei Semibold"),
        },
        new NavigationViewItem()
        {
            Content = "宿舍拼图",
            Icon = new SymbolIcon { Symbol = SymbolRegular.TetrisApp24, FontWeight = FontWeights.SemiBold },
            TargetPageType = typeof(Views.Pages.TetrisPage),
            FontFamily = new FontFamily("Microsoft YaHei Semibold"),
        },
        //new NavigationViewItem()
        //{
        //    Content = "自动化",
        //    Icon = new SymbolIcon { Symbol = SymbolRegular.ApprovalsApp20, FontWeight = FontWeights.SemiBold },
        //    TargetPageType = typeof(Views.Pages.AutoControlPage),
        //    FontFamily = new FontFamily("Microsoft YaHei Semibold"),
        //}
    ];

    [ObservableProperty]
    private ObservableCollection<object> _footerMenuItems =
    [
        new NavigationViewItem()
        {
            Content = "设置",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24, FontWeight = FontWeights.SemiBold },
            TargetPageType = typeof(Views.Pages.SettingsPage),
            FontFamily = new FontFamily("Microsoft YaHei Semibold"),
        }
    ];
}
