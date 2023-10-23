using SnowbreakGachaExport.Models;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace SnowbreakGachaExport.Tools;

/// <summary>
/// 使用像素值进行记录查找
/// </summary>
public static class PxFind
{
    private static PooledWrappedBitmap PreviousLogView;

    public static async Task<ImmutableArray<HistoryItem>> IdentifyHistories(BitMapPool _bitMapPool, AppConfig _config)
    {
        var items = ImmutableArray.CreateBuilder<HistoryItem>();

        while (true)
        {
            var logView = WindowOperate.CaptureHistoryRegion(_bitMapPool, _config);

            if (logView is PooledWrappedBitmap pooledBitmap)
            {
                if (PreviousLogView != null)
                {
                    var mse = CalculateMSE(PreviousLogView.Inner, pooledBitmap.Inner);
                    PreviousLogView.Dispose();
                    if (mse < _config.ImageMesThr)
                    {
                        pooledBitmap.Dispose();
                        break;
                    }
                }

                var nameBitmaps = await CaptureAreasAsync(_bitMapPool, pooledBitmap.Inner.Bitmap, _config.ClientNameRects);
                var timeBitmaps = await CaptureAreasAsync(_bitMapPool, pooledBitmap.Inner.Bitmap, _config.ClientTimeRects);
                var rarePixels = await GetPixelsAsync(pooledBitmap.Inner.Bitmap, _config);

                PreviousLogView = pooledBitmap;

                items.AddRange(await IdentifyItemsAsync(nameBitmaps, timeBitmaps, rarePixels, _config));

                for (int i = 0; i < nameBitmaps.Length; i++)
                {
                    nameBitmaps[i].Inner.Bitmap.Save($"./Images/{i}.png", System.Drawing.Imaging.ImageFormat.Png);
                    nameBitmaps[i].Dispose();
                }
                foreach (var bitmap in timeBitmaps) bitmap.Dispose();
            }

            MouseOperate.DoMouseClick(_config.ClientNextPageArrowPoint.X, _config.ClientNextPageArrowPoint.Y);

            await Task.Delay(200);
        }

        return items.ToImmutable();
    }

    /// <summary>
    /// 截取名称或抽取时间区域截图
    /// </summary>
    /// <param name="_bitMapPool"></param>
    /// <param name="srcImage"></param>
    /// <param name="rects"></param>
    /// <returns></returns>
    private static Task<ImmutableArray<PooledWrappedBitmap>> CaptureAreasAsync(BitMapPool _bitMapPool, Bitmap srcImage, ImmutableArray<Rectangle> rects)
    {
        var images = ImmutableArray.CreateBuilder<PooledWrappedBitmap>();

        foreach (var rect in rects)
        {
            var bitmap = _bitMapPool.Rent(rect);
            using var g = Graphics.FromImage(bitmap.Inner.Bitmap);
            g.DrawImage(srcImage, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);

            images.Add(bitmap);
        }

        return Task.FromResult(images.ToImmutable());
    }

    /// <summary>
    /// 获取每条记录的稀有度颜色
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="_config"></param>
    /// <returns></returns>
    private static Task<ImmutableArray<Color>> GetPixelsAsync(Bitmap bitmap, AppConfig _config)
    {
        var pixels = ImmutableArray.CreateBuilder<Color>();

        foreach (var point in _config.ClientRarePoints)
        {
            var color = bitmap.GetPixel(point.X, point.Y);
            pixels.Add(color);
        }

        return Task.FromResult(pixels.ToImmutable());
    }

    /// <summary>
    /// 识别所有物品
    /// </summary>
    /// <param name="nameBitmaps"></param>
    /// <param name="timeBitmaps"></param>
    /// <param name="rareColors"></param>
    /// <param name="_config"></param>
    /// <returns></returns>
    public static Task<ImmutableArray<HistoryItem>> IdentifyItemsAsync(ImmutableArray<PooledWrappedBitmap> nameBitmaps, 
        ImmutableArray<PooledWrappedBitmap> timeBitmaps, ImmutableArray<Color> rareColors, AppConfig _config)
    {
        var items = ImmutableArray.CreateBuilder<HistoryItem>();

        var (names, times) = TesseractOperate.GetNameAndTime(nameBitmaps, timeBitmaps);

        for (int i = 0; i < names.Length; i++)
        {
            var star = IdentifyRareFromColor(rareColors[i], _config);
            if (star == 0) break;
            items.Add(new HistoryItem(name:names[i], times[i], star: star));

            Debug.WriteLine($"{names[i]}, {times[i]}, {items[i].Star}");
        }

        return Task.FromResult(items.ToImmutable());
    }

    private static int IdentifyRareFromColor(Color color, AppConfig _confgi)
    {
        var blueMSE = ColorMSE(_confgi.RareBlueColor, color);
        var purpleMSE = ColorMSE(_confgi.RarePurpleColor, color);
        var goldMSE = ColorMSE(_confgi.RareGoldColor, color);

        if (Math.Min(blueMSE, Math.Min(purpleMSE, goldMSE)) >= 45) return 0;

        if (goldMSE < purpleMSE && goldMSE < blueMSE) return 5;
        if (purpleMSE < goldMSE && purpleMSE < blueMSE) return 4;
        return 3;
    }

    private static double ColorMSE(Color a, Color b)
    {
        return Vector3.DistanceSquared(new Vector3(a.R, a.G, b.R), new Vector3(b.R, b.G, b.B));
    }

    private static double CalculateMSE(WrappedBitmap bmp1, WrappedBitmap bmp2)
    {
        double mes = 0;

        var rect = new Rectangle(0, 0, bmp1.Bitmap.Width, bmp1.Bitmap.Height);

        var data1 = bmp1.Bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var data2 = bmp2.Bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        unsafe
        {
            for (int y = 0; y < rect.Height; y++)
            {
                for (int x = 0; x < rect.Width; x++)
                {
                    var color1 = Color.FromArgb(*(int*)(data1.Scan0 + data1.Stride * y + x * 4));
                    var color2 = Color.FromArgb(*(int*)(data2.Scan0 + data2.Stride * y + x * 4));

                    var cv1 = new Vector3(color1.R, color1.G, color1.B);
                    var cv2 = new Vector3(color2.R, color2.G, color2.B);

                    mes += Vector3.DistanceSquared(cv1, cv2);
                }
            }
        }

        bmp1.Bitmap.UnlockBits(data1);
        bmp2.Bitmap.UnlockBits(data2);

        double mesPerPixel = mes / (rect.Width * rect.Height * 3);
        return mesPerPixel;
    }
}
