using ReactiveUI;

namespace SnowbreakGachaExport.ViewModels;

public class ViewModelBase : ReactiveObject
{
}

public class MainPageViewModelBase : ViewModelBase
{
    public string NavHeader { get; set; }

    public string IconKey { get; set; }

    public bool ShowInFooter { get; set; }
}