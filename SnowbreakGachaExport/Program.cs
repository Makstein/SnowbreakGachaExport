using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;

namespace SnowbreakGachaExport;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Directory.CreateDirectory("./Data/Logs");
            var fileName = "CrashLog" + DateTime.Now.ToString("yy-MM-dd-hh-mm-ss") + ".log";

            var fs = File.Create(Path.Combine("./Data/Logs", fileName));
            var bytes = System.Text.Encoding.UTF8.GetBytes(e.Message);
            fs.Write(bytes);
            fs.Dispose();
            
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}