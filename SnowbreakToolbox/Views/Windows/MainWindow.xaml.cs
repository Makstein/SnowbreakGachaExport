// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.ViewModels.Windows;
using System.Windows.Interop;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.Views.Windows;

public partial class MainWindow : FluentWindow, INavigationWindow
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel,
        IPageService pageService,
        INavigationService navigationService,
        ISnowbreakConfig configService,
        IContentDialogService contentDialogService)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        // Fix the title bar color.
        //new WindowInteropHelper(this).EnsureHandle();
        //SystemThemeWatcher.Watch(this);

        // Use user prefer theme
        var config = configService.GetConfig();
        var currentTheme = ApplicationThemeManager.GetAppTheme();
        switch (config.UserPreferTheme)
        {
            case "Light":
                if (currentTheme == ApplicationTheme.Light)
                    break;
                ApplicationThemeManager.Apply(ApplicationTheme.Light);
                break;
            default:
                if (currentTheme == ApplicationTheme.Dark)
                    break;
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                break;
        }

        SetPageService(pageService);
        navigationService.SetNavigationControl(RootNavigation);

        contentDialogService.SetDialogHost(RootContentDialog);
    }

    #region INavigationWindow methods

    public INavigationView GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService) => RootNavigation.SetPageService(pageService);

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    #endregion INavigationWindow methods

    /// <summary>
    /// Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    INavigationView INavigationWindow.GetNavigation()
    {
        throw new NotImplementedException();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
}
