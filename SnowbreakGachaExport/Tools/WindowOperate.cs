using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace SnowbreakGachaExport.Tools;

public class WindowOperate
{
    private delegate bool WndEnumProc(IntPtr hWnd, int lParam);
    
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string className, string windowTitle);

    [DllImport("user32")]
    private static extern bool EnumWindows(WndEnumProc lpEnumFunc, int lParam);

    [DllImport("user32")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lptrString, int nMaxCount);
    
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);
    
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);
    
    [DllImport("user32.dll")]
    private static extern int SetForegroundWindow(IntPtr hwnd);
    
    [DllImport("user32")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    
    private enum ShowWindowEnum
    {
        Hide = 0,
        ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
        Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
        Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
        Restore = 9, ShowDefault = 10, ForceMinimized = 11
    };

    
    [StructLayout(LayoutKind.Sequential)]
    private struct WindowPlacement
    {
        public int length;
        public int flags;
        public int showCmd;
        public System.Drawing.Point ptMinPosition;
        public System.Drawing.Point ptMaxPosition;
        public System.Drawing.Rectangle rcNormalPosition;
    }

    public static List<string> FindAll()
    {
        var res = new List<string>();
        EnumWindows(OnWindowEnum, 0);

        bool OnWindowEnum(IntPtr hwnd, int param)
        {
            if (GetParent(hwnd) == IntPtr.Zero)
            {
                if (!IsWindowVisible(hwnd)) return true;
                
                var titleTemp = new StringBuilder(512);
                GetWindowText(hwnd, titleTemp, titleTemp.Capacity);
                var title = titleTemp.ToString().Trim();
                if (title.Length > 0)
                {
                    res.Add(title);
                }
            }

            return true;
        }

        return res;
    }

    public static void BringToFront(string title)
    {
        var hwnd = FindWindow(null, title);

        var windowPlacement = new WindowPlacement();
        GetWindowPlacement(hwnd, ref windowPlacement);
        
        if (windowPlacement.showCmd != (int)ShowWindowEnum.Restore)
        {
            ShowWindow(hwnd, ShowWindowEnum.Restore);
        }

        SetForegroundWindow(hwnd);
    }

    public static Bitmap GetScreenShot()
    {
        try
        {
            var image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
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
            var rc = new Rectangle(x - 10, y - 80, 30, 80);
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