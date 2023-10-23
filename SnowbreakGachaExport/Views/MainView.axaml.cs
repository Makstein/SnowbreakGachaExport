using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using SnowbreakGachaExport.Services;
using SnowbreakGachaExport.ViewModels;
using System.Collections.Generic;

namespace SnowbreakGachaExport.Views;

public partial class MainView : Avalonia.Controls.UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var vm = new MainViewViewModel();
        DataContext = vm;
        FrameView.NavigationPageFactory = vm.NavigationFactory;
        NavigationService.Instance.SetFrame(FrameView);

        InitializeNavigationPages();

        NavView.ItemInvoked += OnNavigationViewItemInvoked;
    }

    private void InitializeNavigationPages()
    {
        var mainPages = new MainPageViewModelBase[]
        {
            new GachaLogViewModel()
            {
                NavHeader = "GachaLog",
                ShowInFooter = false,
                IconKey = "CoreControlsIcon"
            },
            new SettingViewModel()
            {
                NavHeader = "Settings",
                ShowInFooter = true,
                IconKey = "SettingIcon"
            }
        };

        var menuItems = new List<NavigationViewItemBase>(1);
        var footerItems = new List<NavigationViewItemBase>(1);

        bool inDesign = Design.IsDesignMode;

        Dispatcher.UIThread.Post(() =>
        {
            for (int i = 0; i < mainPages.Length; i++)
            {
                var pg = mainPages[i];
                var nvi = new NavigationViewItem
                {
                    Content = pg.NavHeader,
                    Tag = pg,
                    IconSource = this.FindResource(pg.IconKey) as IconSource
                };

                nvi.Classes.Add("SampleAppNav");

                if (pg.ShowInFooter)
                    footerItems.Add(nvi);
                else
                    menuItems.Add(nvi);
            }

            NavView.MenuItemsSource = menuItems;
            NavView.FooterMenuItemsSource = footerItems;

            NavView.Classes.Add("SampleAppNav");

            FrameView.NavigateFromObject((NavView.MenuItemsSource.ElementAt(0) as Control)?.Tag);
        });
    }

    private void OnNavigationViewItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is NavigationViewItem nvi)
        {
            NavigationTransitionInfo info;

            NavigationService.Instance.NavigateFromContext(nvi.Tag);
        }
    }
}
