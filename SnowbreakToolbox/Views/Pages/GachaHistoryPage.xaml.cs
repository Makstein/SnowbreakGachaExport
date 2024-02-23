using SnowbreakToolbox.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.Views.Pages;

/// <summary>
/// GachaHistoryPage.xaml 的交互逻辑
/// </summary>
public partial class GachaHistoryPage : INavigableView<GachaHistoryViewModel>
{
    public GachaHistoryViewModel ViewModel { get; }

    public GachaHistoryPage(GachaHistoryViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
