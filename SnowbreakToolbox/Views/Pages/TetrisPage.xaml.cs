using Wpf.Ui.Controls;

namespace SnowbreakToolbox.Views.Pages;

/// <summary>
/// TetrisPage.xaml 的交互逻辑
/// </summary>
public partial class TetrisPage : INavigableView<ViewModels.Pages.TetrisViewModel>
{
    public ViewModels.Pages.TetrisViewModel ViewModel { get; }

    public TetrisPage(ViewModels.Pages.TetrisViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
