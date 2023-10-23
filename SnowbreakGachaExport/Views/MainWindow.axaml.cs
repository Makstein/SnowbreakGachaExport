using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SnowbreakGachaExport.ViewModels;
using System.Collections.Generic;

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