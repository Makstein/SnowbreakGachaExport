using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Windowing;

namespace SnowbreakGachaExport.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        //InitializeComponent();

        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        TitleBar.ExtendsContentIntoTitleBar = true;
    }
}