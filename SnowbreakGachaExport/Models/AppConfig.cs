using MsBox.Avalonia;
using System;
using System.Collections.Immutable;
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
            msgBox.ShowWindowAsync();
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
        var regionName = new Rectangle(NameAreaX0, NameAreaY0, NameAreaWidth, NameAreaHeight);
        var reegionTime = new Rectangle(TimeAreaX0, TimeAreaY0, TimeAreaWidth, TimeAreaHeight);

        ClientRegionLog = TranslateRectangleCelling(regionLog);

        ClientSingleLogHeight = SingleLogHeight * ClientScale;

        ClientRareAreaMarginLeft = (int)(RareAreaMarginLeft * ClientScale);
        ClientRareAreaMarginTop = (int)(RareAreaMarginTop * ClientScale);

        ClientNameAreaX0 = (int)(NameAreaX0 * ClientScale);
        ClientNameAreaY0 = (int)(NameAreaY0 * ClientScale);
        ClientNameAreaWidth = (int)(NameAreaWidth * ClientScale);
        ClientNameAreaHeight = (int)(NameAreaHeight * ClientScale);

        ClientTimeAreaX0 = (int)(TimeAreaX0 * ClientScale);
        ClientTimeAreaY0 = (int)(TimeAreaY0 * ClientScale);
        ClientTimeAreaWidth = (int)(TimeAreaWidth * ClientScale);
        ClientTimeAreaHeight = (int)(TimeAreaHeight * ClientScale);

        var clientNameRects = ImmutableArray.CreateBuilder<Rectangle>(10);
        var clientTimeRects = ImmutableArray.CreateBuilder<Rectangle>(10);
        var clientRarePoints = ImmutableArray.CreateBuilder<Point>(10);

        for (int i = 0; i < 10 ; i++)
        {
            clientNameRects.Add(Rectangle.Round(new RectangleF(ClientNameAreaX0, i * ClientSingleLogHeight + ClientNameAreaY0,
                ClientNameAreaWidth, ClientNameAreaHeight)));
            clientTimeRects.Add(Rectangle.Round(new RectangleF(ClientTimeAreaX0, i * ClientSingleLogHeight + ClientTimeAreaY0,
                ClientTimeAreaWidth, ClientTimeAreaHeight)));
            clientRarePoints.Add(new Point(ClientRareAreaMarginLeft, (int)(i * ClientSingleLogHeight + ClientRareAreaMarginTop)));
        }

        ClientNameRects = clientNameRects.ToImmutableArray();
        ClientTimeRects = clientTimeRects.ToImmutableArray();
        ClientRarePoints = clientRarePoints.ToImmutableArray();

        ClientNextPageArrowPoint = new Point((int)(ReferenceNextPageArrowX * ClientScale), (int)(ReferenceNextPageArrowY * ClientScale));
    }

    /// <summary>
    /// 从参考区域投影到客户端缩放区域
    /// </summary>
    /// <param name="referenceRect"></param>
    /// <returns></returns>
    private Rectangle TranslateRectangleCelling(Rectangle referenceRect)
    {
        var tMin = Vector2.Transform(new Vector2(referenceRect.X, referenceRect.Y), ClientMatrix);
        var tMax = Vector2.Transform(new Vector2(referenceRect.Right, referenceRect.Bottom), ClientMatrix);

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

    public ImmutableArray<Rectangle> ClientNameRects { get; set; }

    public ImmutableArray<Rectangle> ClientTimeRects { get; set; }

    // 使用颜色采样点判断物品稀有度，此为每页的采样点列表
    public ImmutableArray<Point> ClientRarePoints { get; set; }

    public float ClientSingleLogHeight { get; set; }

    public int ClientRareAreaMarginLeft { get; set; }

    public int ClientRareAreaMarginTop { get; set; }

    public int ClientNameAreaX0 { get; set; }

    public int ClientNameAreaY0 { get; set; }
    
    public int ClientNameAreaWidth { get; set; }

    public int ClientNameAreaHeight { get; set; }

    public int ClientTimeAreaX0 { get; set; }
    
    public int ClientTimeAreaY0 { get; set; }

    public int ClientTimeAreaWidth { get; set; }

    public int ClientTimeAreaHeight { get; set; }

    public Point ClientNextPageArrowPoint { get; set; }
    
    public bool IsInit { get; set; }

    public double ImageMesThr { get; set; } = 45;

    public Color RareBlueColor { get; set; } = Color.FromArgb(255, 55, 98, 242);

    public Color RarePurpleColor { get; set; } = ColorTranslator.FromHtml("#c069d6");

    public Color RareGoldColor { get; set; } = ColorTranslator.FromHtml("#e99b37");

    private const float ReferenceWidth = 1920;
    private const float ReferenceHeight = 1080;

    // 历史记录区域范围
    private const int LogBoxX0 = 325;
    private const int LogBoxY0 = 188;
    private const int LogBoxWidth = 1260;
    private const int LogBoxHeight = 680;

    // 历史记录物品名称区域
    private const int NameAreaX0 = 30;
    private const int NameAreaY0 = 5;
    private const int NameAreaWidth = 170;
    private const int NameAreaHeight = 50;

    // 历史记录物品抽取时间区域
    private const int TimeAreaX0 = 1050;
    private const int TimeAreaY0 = 18;
    private const int TimeAreaWidth = 180;
    private const int TimeAreaHeight = 30;

    // 单条指令高度
    private const int SingleLogHeight = 68;

    private const int RareAreaMarginLeft = 27;
    private const int RareAreaMarginTop = 35;

    // 下一页按钮位置
    private const int ReferenceNextPageArrowX = 1665;
    private const int ReferenceNextPageArrowY = 600;
}