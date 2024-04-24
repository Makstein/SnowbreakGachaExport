using Microsoft.Win32;
using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Models;
using SnowbreakToolbox.Tools;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Vanara.PInvoke;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject, INavigationAware, IDisposable
{
    private readonly StackPanel SelectGamePathPanel = new();

    private AppConfig? _config;
    private IContentDialogService? _contentDialogService;
    private HWND _gameHwnd;

    [ObservableProperty]
    private string _dialogGamePath = string.Empty;

    private bool _initialized = false;

    public void Dispose()
    {
        CloseLauncher();
        GC.SuppressFinalize(this);
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

            InitSelectGamePanel();
        }
        catch (Exception ex)
        {
            _initialized = false;
            Log.Error(ex, "DashBoardViewModel initialize failed");
        }
    }

    /// <summary>
    /// Generate [Select game path] window in code
    /// </summary>
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

    [RelayCommand]
    private async Task RunGame(string param)
    {
        try
        {
            if (_config == null) { throw new InvalidOperationException("Dashboard read config file failed"); }

            if (_config.GamePlatform == GamePlatform.Steam)
            {
                var process = new Process()
                {
                    StartInfo =
                    {
                        UseShellExecute = true,
                        FileName = @"steam://rungameid/" + _config.GameSteamId
                    }
                };
                process.Start();
                return;
            }

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

#if DEBUG
            // Display all windows titles
            User32.EnumWindows((hwnd, param) =>
            {
                if (!User32.IsWindowVisible(hwnd)) return true;

                var titleTemp = new StringBuilder(128);
                if (User32.GetWindowText(hwnd, titleTemp, 128) == 0) return true;
                var title = titleTemp.ToString().Trim();
                Debug.WriteLine(title);

                return true;
            }, 0);
#endif

            // Start launcher process
            Process.Start(Path.Combine(_config.GamePath, _config.LauncherExeFileName));

            if (param == "Launcher") return;

            // Try click the run button until the game started
            await Task.Run(() =>
            {
                // Wait for launcher appear
                var launcherHwnd = User32.FindWindow(null, _config.LauncherWindowTitle);
                while (launcherHwnd == HWND.NULL)
                {
                    launcherHwnd = User32.FindWindow(null, _config.LauncherWindowTitle);
                    Task.Delay(500).Wait();
                }

                // Get "Run Game" button position
                User32.GetWindowRect(launcherHwnd, out var rect);
                var clientLauncherStartBtnPosX = rect.Left + _config.LauncherStartBtnPosX;
                var clientLauncherStartBtnPosY = rect.top + _config.LauncherStartBtnPosY;

                // Try run the game
                var count = 0;
                while ((User32.FindWindow(null, _config.GameWindowTitle) == HWND.NULL) && (User32.FindWindow(null, _config.GameWindowTitleCN) == HWND.NULL))
                {
                    if (count > 7)
                    {
                        throw new Exception("Exceed max retry time, can't find game window");
                    }

                    MouseOperations.LeftMouseClick(clientLauncherStartBtnPosX, clientLauncherStartBtnPosY);
                    count++;
                    Task.Delay(1000).Wait();
                }
            });

            if (!_config.CloseLauncherWhenGameExit) return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _gameHwnd = User32.FindWindow(null, _config.GameWindowTitle);
                if (_gameHwnd == HWND.NULL)
                {
                    _gameHwnd = User32.FindWindow(null, _config.GameWindowTitleCN);
                }

                var threadID = User32.GetWindowThreadProcessId(_gameHwnd, out var processId);
                if (threadID == 0) return;
                var hookInstance = User32.SetWinEventHook(User32.EventConstants.EVENT_OBJECT_DESTROY, User32.EventConstants.EVENT_OBJECT_DESTROY, HINSTANCE.NULL, WinEventProc, processId, threadID, User32.WINEVENT.WINEVENT_OUTOFCONTEXT);
                Debug.WriteLine(hookInstance == IntPtr.Zero ? "Hook failed" : "Hook success");
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Launch failed");
        }
    }

    /// <summary>
    /// Callback when game exit
    /// </summary>
    /// <param name="hWinEventHook"></param>
    /// <param name="winEvent"></param>
    /// <param name="hwnd"></param>
    /// <param name="idObject"></param>
    /// <param name="idChild"></param>
    /// <param name="idEventThread"></param>
    /// <param name="dwmsEventTime"></param>
    private void WinEventProc(User32.HWINEVENTHOOK hWinEventHook, uint winEvent, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
        if (winEvent != User32.EventConstants.EVENT_OBJECT_DESTROY) return;
        if (hwnd != _gameHwnd) return;
        if (idObject != User32.ObjectIdentifiers.OBJID_WINDOW) return;

        Debug.WriteLine("Game Exit");

        CloseLauncher(true);
    }

    private void CloseLauncher(bool isWaitForLauncher = false)
    {
        if (_config != null)
        {
            var hwnd = User32.FindWindow(null, _config.LauncherWindowTitle);

            if (isWaitForLauncher)
            {
                var count = 0;
                while (hwnd != IntPtr.Zero)
                {
                    if (count > 10)
                        return;

                    hwnd = User32.FindWindow(null, _config.LauncherWindowTitle);
                    count++;
                    Task.Delay(1000).Wait();
                }
            }

            if (hwnd == IntPtr.Zero)
                return;

            User32.SendMessage(hwnd, User32.WindowMessage.WM_CLOSE);
        }
    }
}