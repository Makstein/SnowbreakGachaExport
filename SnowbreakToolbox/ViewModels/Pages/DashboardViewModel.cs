using Microsoft.Win32;
using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using SnowbreakToolbox.Tools;
using System.Windows.Controls;
using System.Windows.Data;
using Vanara.PInvoke;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject, INavigationAware
{
    private readonly StackPanel SelectGamePathPanel = new();

    private bool _initialized = false;
    private AppConfig? _config;
    private IContentDialogService? _contentDialogService;

    [ObservableProperty]
    private string _dialogGamePath = string.Empty;

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

            InitSelectGamePanel();
        }
        catch (Exception ex)
        {
            _initialized = false;
            Log.Error(ex, "DashBoardViewModel initialize failed");
        }
    }

    private void InitSelectGamePanel()
    {
        Wpf.Ui.Controls.TextBlock textBlock = new()
        {
            Text = "路径：",
            VerticalAlignment = VerticalAlignment.Center,
        };
        var textBinding = new Binding("DialogGamePath")
        {
            Source = this,
        };
        TextBox textBox = new()
        {
            Margin = new Thickness(15, 0, 0, 0),
            MinWidth = 350
        };
        textBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, textBinding);
        Wpf.Ui.Controls.Button button = new()
        {
            Margin = new Thickness(15, 0, 0, 0),
            Command = SelectGameFolderCommand,
            Content = "..."
        };
        SelectGamePathPanel.VerticalAlignment = VerticalAlignment.Center;
        SelectGamePathPanel.HorizontalAlignment = HorizontalAlignment.Center;
        SelectGamePathPanel.Orientation = Orientation.Horizontal;
        SelectGamePathPanel.Children.Add(textBlock);
        SelectGamePathPanel.Children.Add(textBox);
        SelectGamePathPanel.Children.Add(button);
        SelectGamePathPanel.Visibility = Visibility.Visible;
    }

    [RelayCommand]
    private async Task RunGame(string param)
    {
        try
        {
            if (_config == null) { throw new InvalidOperationException("DashBoard read config file failed"); }

            if (string.IsNullOrEmpty(_config.GamePath))
            {
                ContentDialogResult result = await _contentDialogService!.ShowSimpleDialogAsync(
                    new SimpleContentDialogCreateOptions()
                    {
                        Title = "选择游戏路径",
                        Content = SelectGamePathPanel,
                        PrimaryButtonText = "启动",
                        CloseButtonText = "取消"
                    }
                );

                if (result == ContentDialogResult.Secondary) { return; }

                _config.GamePath = DialogGamePath;
                App.GetService<ISnowbreakConfig>()?.SetConfig(_config);
            }

            //using var p = new Process();
            //p.StartInfo = new ProcessStartInfo()
            //{
            //    UseShellExecute = false,
            //    CreateNoWindow = false,
            //    FileName = Path.Combine(_config.GamePath, _config.LauncherExeFileName),
            //};
            //p.Start();

            await Task.Run(() =>
            {
                while (User32.FindWindow(_config.GameWindowTitle) == HWND.NULL)
                {
                    Task.Delay(500).Wait();
                }
            }).ContinueWith((res) =>
            {
                MouseOperations.LeftMouseClick(_config.LauncherStartBtnPosX, _config.LauncherStartBtnPosY);
            });

        }
        catch (Exception ex)
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
