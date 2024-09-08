using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Services;
using SnowbreakToolbox.ViewModels.Pages;
using SnowbreakToolbox.ViewModels.Windows;
using SnowbreakToolbox.Views.Pages;
using SnowbreakToolbox.Views.Windows;
using System.IO;
using System.Windows.Threading;
using Wpf.Ui;

namespace SnowbreakToolbox;

/***
 *                    _ooOoo_
 *                   o8888888o
 *                   88" . "88
 *                   (| -_- |)
 *                    O\ = /O
 *                ____/`---'\____
 *              .   ' \\| |// `.
 *               / \\||| : |||// \
 *             / _||||| -:- |||||- \
 *               | | \\\ - /// | |
 *             | \_| ''\---/'' | |
 *              \ .-\__ `-` ___/-. /
 *           ___`. .' /--.--\ `. . __
 *        ."" '< `.___\_<|>_/___.' >'"".
 *       | | : `- \`.;`\ _ /`;.`/ - ` : | |
 *         \ \ `-. \_ __\ /__ _/ .-` / /
 * ======`-.____`-.___\_____/___.-`____.-'======
 *                    `=---='
 *
 * .............................................
 *          佛祖保佑             永无BUG
 */

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)!); })
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<ApplicationHostService>();
            // Page resolver service
            services.AddSingleton<IPageService, PageService>();
            // Theme manipulation
            services.AddSingleton<IThemeService, ThemeService>();
            // TaskBar manipulation
            services.AddSingleton<ITaskBarService, TaskBarService>();
            // Service containing navigation, same as INavigationWindow... but without window
            services.AddSingleton<INavigationService, NavigationService>();
            // Dialog
            services.AddSingleton<IContentDialogService, ContentDialogService>();
            services.AddSingleton<ISnowbreakOcr, PaddleOrcService>();
            services.AddSingleton<ISnowbreakConfig, ConfigService>();
            services.AddSingleton<ISnowbreakHistory, HistoryService>();
            services.AddSingleton<IModService, ModService>();

            // Suppress the .net host messages when starting. ("...press Ctrl + c to stop...", etc)
            services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

            // Config logging
            var logPath = Path.Combine(AppContext.BaseDirectory, "Logs");
            var logFile = Path.Combine(logPath, "log_.txt");
            var serilogOutputTemplate = "[{Timestamp:HH:mm:ss:fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFile, outputTemplate: serilogOutputTemplate, rollingInterval: RollingInterval.Day)
                .MinimumLevel.Information()
                .CreateLogger();
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

            // Main window with navigation
            services.AddSingleton<INavigationWindow, MainWindow>();
            services.AddSingleton<MainWindowViewModel>();

            services.AddSingleton<DashboardPage>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<GachaHistoryPage>();
            services.AddSingleton<GachaHistoryViewModel>();
            services.AddSingleton<TetrisPage>();
            services.AddSingleton<TetrisViewModel>();
            services.AddSingleton<AutoControlPage>();
            services.AddSingleton<AutoControlViewModel>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<ModManagerPage>();
            services.AddSingleton<ModManagerViewModel>();
        }).Build();

    /// <summary>
    /// Gets registered service.
    /// </summary>
    /// <typeparam name="T">Type of the service to get.</typeparam>
    /// <returns>Instance of the service or <see langword="null"/>.</returns>
    public static T? GetService<T>()
        where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0

        Log.Error(e.Exception, "ApplicationDispatcherUnhandledException");

        var msgBox = new Wpf.Ui.Controls.MessageBox()
        {
            Title = "错误",
            Content = e.Exception.Message,
            CloseButtonText = "退出",
        };
        msgBox.ShowDialogAsync();
    }

    /// <summary>
    /// Occurs when the application is closing.
    /// </summary>
    private async void OnExit(object sender, ExitEventArgs e)
    {
        GetService<ISnowbreakConfig>()!.Save();
        GetService<ModManagerViewModel>()!.Dispose();
        GetService<IModService>()!.Save();
        GetService<DashboardViewModel>()!.Dispose();
        GetService<GachaHistoryViewModel>()!.Dispose();

        await _host.StopAsync();

        _host.Dispose();
    }

    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.Start();
    }
}