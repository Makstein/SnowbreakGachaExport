using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SnowbreakGachaExport.Views.Pages;
using System;

namespace SnowbreakGachaExport.ViewModels;

public class MainViewViewModel : ViewModelBase
{
    public NavigationFactory NavigationFactory { get; }

    public MainViewViewModel()
    {
        NavigationFactory = new NavigationFactory(this);
    }
}

public class NavigationFactory : INavigationPageFactory
{
    public MainViewViewModel Owner { get; }

    public NavigationFactory(MainViewViewModel owner) => Owner = owner;

    public Control? GetPage(Type srcType)
    {
        return null;
    }

    public Control GetPageFromObject(object target)
    {
        return target switch
        {
            GachaLogViewModel => new GachaLogPage { DataContext = target },
            SettingViewModel => new SettingPage { DataContext = target },
            _ => throw new Exception("Unsolved ViewModel")
        };
    }
}
