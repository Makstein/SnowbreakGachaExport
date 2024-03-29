﻿using SnowbreakToolbox.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.Views.Pages;

/// <summary>
/// AutoControlPage.xaml 的交互逻辑
/// </summary>
public partial class AutoControlPage : INavigableView<AutoControlViewModel>
{
    public AutoControlViewModel ViewModel { get; }

    public AutoControlPage(AutoControlViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
