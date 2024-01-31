using SnowbreakGachaExport.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Windows.Win32;

namespace SnowbreakGachaExport.Tools;

public static partial class WindowOperate
{
    [DllImport("user32", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lptrString, int nMaxCount);

    public static List<string> FindAll()
    {
        var res = new List<string>();

        PInvoke.EnumWindows((hwnd, param) =>
        {
            if (PInvoke.GetParent(hwnd) != IntPtr.Zero) return true;
            if (!PInvoke.IsWindowVisible(hwnd)) return true;

            var titleTemp = new StringBuilder(128);
            if (GetWindowText(hwnd, titleTemp, 128) == 0) return true;
            var title = titleTemp.ToString().Trim();
            res.Add(title);

            return true;
        }, 0);

        return res;
    }

    public static void BringToFront(string title)
    {
        var hwnd = PInvoke.FindWindow(null, title);

        PInvoke.SetForegroundWindow(hwnd);
    }

    public static Bitmap GetScreenShot()
    {
        try
        {
            var image = new Bitmap(Screen.PrimaryScreen!.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            var g = Graphics.FromImage(image);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.PrimaryScreen.Bounds.Size);
            image.Save("./Images/123.png", ImageFormat.Png);
            return image;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static IFullImage CaptureHistoryRegion(BitMapPool bitMapPool, AppConfig config)
    {
        var bitmap = bitMapPool.Rent(config.ClientRegionLog);
        using var g = Graphics.FromImage(bitmap.Inner.Bitmap);
        g.CopyFromScreen(config.ClientRegionLog.Left, config.ClientRegionLog.Top, 0, 0, config.ClientRegionLog.Size);
        return bitmap;
    }

    public static Bitmap GetItemTimeScreenshot(int x, int y, int height)
    {
        try
        {
            var rc = new Rectangle(x + 1000, y, 300, height);
            var bitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            
            using var g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            
            bitmap.Save("./Images/ItemTime.png", ImageFormat.Png);
            
            return bitmap;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static Bitmap GetCharacterNameScreenshot(int x, int y, int height)
    {
        try
        {
            var rc = new Rectangle(x + 30, y, 500, height);
            var bitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            
            using var g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            
            bitmap.Save("./Images/789.png", ImageFormat.Png);
            
            return bitmap;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static Bitmap GetPageScreenshot(int x, int y, int width)
    {
        try
        {
            var rc = new Rectangle(x - 10, y - 80, 70, 80);
            var bitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            
            using var g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            
            bitmap.Save("./Images/456.png", ImageFormat.Png);
            
            return bitmap;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error in GetPageScreenshot: " + e);
            throw;
        }
    }
    
    
}