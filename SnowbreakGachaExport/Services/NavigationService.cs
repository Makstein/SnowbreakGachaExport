using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using System;

namespace SnowbreakGachaExport.Services;

public class NavigationService
{
    public static NavigationService Instance { get; } = new NavigationService();

    public Control PreviousPage { get; set; }

    private Frame _frame;

    public void SetFrame(Frame f) => _frame = f;

    public void Navigate(Type t) => _frame.Navigate(t);

    public void NavigateFromContext(object dataContext, NavigationTransitionInfo transitionInfo = null)
    {
        _frame.NavigateFromObject(dataContext,
            new FluentAvalonia.UI.Navigation.FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            });
    }
}
