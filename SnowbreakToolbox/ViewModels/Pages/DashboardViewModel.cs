using Microsoft.Win32;
using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject, INavigationAware
{
    private bool _initialized = false;
    private AppConfig? _config;
    private IContentDialogService? _contentDialogService;

    private string _dialogGamePath = string.Empty;
    // Only used when not set game launcher path yet
    public string DialogGamePath
    {
        get => _dialogGamePath;
        set
        {
            if (_dialogGamePath == value) { return; }

            _dialogGamePath = value;
            _config!.GamePath = value;
            App.GetService<ISnowbreakConfig>()?.SetConfig(_config);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo()
    {
        if (!_initialized)
            InitializeViewModel();

        // Reload config every time, for hot reload
        _config = App.GetService<ISnowbreakConfig>()?.GetConfig();
    }

    private void InitializeViewModel()
    {
        try
        {
            _initialized = true;
            _contentDialogService = App.GetService<IContentDialogService>();
        }
        catch (Exception ex)
        {
            _initialized = false;
            Log.Error(ex, "DashBoardViewModel initialize failed");
        }
    }

    [RelayCommand]
    private async Task RunGame(object param)
    {
        try
        {
            if (_config == null) { throw new InvalidOperationException("DashBoard read config file failed"); }

            if (param is not object[] values) return;
            object content = values[0];
            string launchParam = (values[1] as string)!;

            if (string.IsNullOrEmpty(_config.GamePath))
            {
                ContentDialogResult result = await _contentDialogService!.ShowSimpleDialogAsync(
                    new SimpleContentDialogCreateOptions()
                    {
                        Title = "选择游戏路径",
                        Content = content,
                        PrimaryButtonText = "启动",
                        CloseButtonText = "取消"
                    }
                );

                if (result == ContentDialogResult.Primary)
                {
                    return;
                }
            }
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Launch failed");
        }
    }

    [RelayCommand]
    private void OnSelectGameFolder()
    {
        OpenFolderDialog openFolderDialog = new()
        {
            Multiselect = false,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };

        if (openFolderDialog.ShowDialog() != true)
            return;

        if (openFolderDialog.FolderNames.Length == 0)
            return;

        DialogGamePath = openFolderDialog.FolderNames[0];
    }
}
