using MsBox.Avalonia;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Windows.Win32;

namespace SnowbreakGachaExport.Models;

public class AppConfig
{
    public void Init(string GameWindowTitle)
    {
        var hWnd = PInvoke.FindWindow(null, GameWindowTitle);
        if (hWnd.IsNull)
        {
            var msgBox =
                MessageBoxManager.GetMessageBoxStandard("", $"Error: cant find game window: '{GameWindowTitle}'");
            _ = msgBox.ShowWindowAsync();
            return;
        }

        PInvoke.GetWindowRect(hWnd, out var rect);
        Debug.WriteLine($"ClientRect: {rect.X}, {rect.Y}, {rect.Width}, {rect.Height}");

        UpdateResolution(rect);

        IsInit = true;
    }

    /// <summary>
    /// 适配客户端分辨率
    /// </summary>
    /// <param name="rect"></param>
    private void UpdateResolution(Rectangle rect)
    {
        ClientRect = rect;

        ClientScale = Math.Min(ClientRect.Width / 16, ClientRect.Height / 9) /
                      Math.Min(ReferenceWidth / 16, ReferenceHeight / 9);

        var offsetX = (ClientRect.Width - ReferenceWidth * ClientScale) / 2 + ClientRect.X;
        var offsetY = (ClientRect.Height - ReferenceHeight * ClientScale) / 2 + ClientRect.Y;

        ClientMatrix = new Matrix3x2(
            ClientScale, 0,
            0, ClientScale,
            offsetX, offsetY);

        var regionLog = new Rectangle(LogBoxX0, LogBoxY0, LogBoxWidth, LogBoxHeight);

        ClientRegionLog = TranslateRectangleCelling(regionLog);
    }

    /// <summary>
    /// 从参考区域投影到客户端缩放区域
    /// </summary>
    /// <param name="referenceRect"></param>
    /// <returns></returns>
    private Rectangle TranslateRectangleCelling(Rectangle referenceRect)
    {
        var tMin = Vector2.Transform(new Vector2(referenceRect.X, referenceRect.Y), ClientMatrix);
        var tMax = Vector2.Transform(new Vector2(referenceRect.Width, referenceRect.Height), ClientMatrix);

        return Rectangle.FromLTRB((int)tMin.X, (int)tMin.Y, (int)float.Ceiling(tMax.X), (int)float.Ceiling(tMax.Y));
    }

    /// <summary>
    /// 客户端缩放倍率
    /// </summary>
    public float ClientScale { get; set; }

    /// <summary>
    /// 客户端窗口区域
    /// </summary>
    public Rectangle ClientRect { get; set; }

    public Matrix3x2 ClientMatrix { get; set; }

    public Rectangle ClientRegionLog { get; set; }
    
    public bool IsInit { get; set; }

    private const float ReferenceWidth = 1920;
    private const float ReferenceHeight = 1080;
    private const int LogBoxX0 = 0;
    private const int LogBoxY0 = 0;
    private const int LogBoxWidth = 0;
    private const int LogBoxHeight = 0;
}