using System;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SnowbreakGachaExport.Models;
using Point = OpenCvSharp.Point;

namespace SnowbreakGachaExport.Tools;

[Obsolete("方法类因过慢已弃用，请使用PxFind类 / Use PxFind class instead, this class is too slow")]
public static class OpenCVFind
{
    private const string nextPageArrowImagePath = "./Images/NextPageArrow.png";
    private const string fiveStarImagePath = "./Images/5Star_Big.png";
    private const string fourStarImagePath = "./Images/4Star_Big.png";
    private const string threeStarImagePath = "./Images/3Star_Big.png";
    private static Mat? lastPage;

    public static Point FindNextPageArrow()
    {
        try
        {
            var arrowMat =
                new Mat(nextPageArrowImagePath, ImreadModes.AnyColor).CvtColor(ColorConversionCodes.RGBA2RGB);
            var screenShot = WindowOperate.GetScreenShot().ToMat().CvtColor(ColorConversionCodes.RGBA2RGB);
            var result = new Mat();

            Cv2.MatchTemplate(screenShot, arrowMat, result, TemplateMatchModes.CCoeffNormed);

            Cv2.MinMaxLoc(result, out _, out var maxVal, out _, out var maxLoc);
            if (maxVal < 0.91) return new Point(0, 0);

            // Compare current page and last page, if same, then done
            var currentPage = WindowOperate.GetPageScreenshot(maxLoc.X, maxLoc.Y, arrowMat.Cols).ToMat()
                .CvtColor(ColorConversionCodes.RGBA2RGB);
            if (lastPage != null)
            {
                var subRes = new Mat();
                Cv2.Subtract(currentPage, lastPage, subRes);
                if (Cv2.CountNonZero(subRes.CvtColor(ColorConversionCodes.BGR2GRAY)) == 0)
                {
                    return new Point(0, 0);
                }
            }

            lastPage = currentPage;

            return new Point(maxLoc.X + arrowMat.Cols / 2, maxLoc.Y + arrowMat.Rows / 2);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public static IEnumerable<HistoryItem> FindStar(ItemType itemType)
    {
        try
        {
            var fiveStarMat = new Mat(fiveStarImagePath, ImreadModes.AnyColor).CvtColor(ColorConversionCodes.RGBA2RGB);
            var fourStarMat = new Mat(fourStarImagePath, ImreadModes.AnyColor).CvtColor(ColorConversionCodes.RGBA2RGB);
            var threeStarMat =
                new Mat(threeStarImagePath, ImreadModes.AnyColor).CvtColor(ColorConversionCodes.RGBA2RGB);

            var screenShot = WindowOperate.GetScreenShot().ToMat().CvtColor(ColorConversionCodes.RGBA2RGB);

            return FindItem(screenShot, fiveStarMat, fourStarMat, threeStarMat, itemType);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// 从上到下循环查找抽卡记录
    /// </summary>
    /// <param name="screenshot"></param>
    /// <param name="fiveStar"></param>
    /// <param name="fourStar"></param>
    /// <param name="threeStar"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private static IEnumerable<HistoryItem> FindItem(Mat screenshot, Mat fiveStar, Mat fourStar, Mat threeStar,
        ItemType type)
    {
        try
        {
            var resList = new List<HistoryItem>();
            const double threshold = 0.91;

            var goldRes = new Mat();
            Cv2.MatchTemplate(screenshot, fiveStar, goldRes, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(goldRes, out _, out double maxGoldVal);

            var purpleRes = new Mat();
            Cv2.MatchTemplate(screenshot, fourStar, purpleRes, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(purpleRes, out _, out double maxPurpleVal);

            var blueRes = new Mat();
            Cv2.MatchTemplate(screenshot, threeStar, blueRes, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(blueRes, out _, out double maxBlueVal);

            for (var i = 1;; i += 70) // 70 代表每条抽卡记录大约高度（内容 + 上下空白一半）
            {
                if (i > goldRes.Rows - fiveStar.Rows && i > purpleRes.Rows - fourStar.Rows &&
                    i > blueRes.Rows - threeStar.Rows) // 到达所有记录底部
                {
                    break;
                }

                if (maxGoldVal > threshold && i < goldRes.Rows - fiveStar.Rows) // 本页有金色物品且当前未超出范围
                {
                    var goldIRect = new Rect(0, i, goldRes.Cols, fiveStar.Rows); // 截取当前一条记录
                    var goldIRes = new Mat(goldRes, goldIRect);

                    // 获得当前记录范围内的最大相似值和最相似坐标值
                    Cv2.MinMaxLoc(goldIRes, out _, out var goldIMaxValue, out _, out var goldIMaxLoc);

                    // 当前范围内是金色物品
                    if (goldIMaxValue > threshold)
                    {
                        var nameBitmap =
                            WindowOperate.GetCharacterNameScreenshot(goldIMaxLoc.X, i + goldIMaxLoc.Y, fiveStar.Rows);
                        var timeBitmap =
                            WindowOperate.GetItemTimeScreenshot(goldIMaxLoc.X, i + goldIMaxLoc.Y, fiveStar.Rows);
                        var nameTime = TesseractOperate.GetNameAndTime(nameBitmap, timeBitmap);

                        resList.Add(new HistoryItem(nameTime.name, nameTime.time, type, 5));
                        continue;
                    }
                }

                if (maxPurpleVal > threshold && i < purpleRes.Rows - fourStar.Rows) // 当前未超出范围且本页有紫色物品
                {
                    var purpleIRect = new Rect(0, i, purpleRes.Cols, fourStar.Rows);
                    var purpleIRes = new Mat(purpleRes, purpleIRect);
                    Cv2.MinMaxLoc(purpleIRes, out _, out var purpleIMaxValue, out _, out var purpleIMaxLoc);

                    // 当前范围内是紫色物品
                    if (purpleIMaxValue > threshold)
                    {
                        var nameBitmap =
                            WindowOperate.GetCharacterNameScreenshot(purpleIMaxLoc.X, i + purpleIMaxLoc.Y, fourStar.Rows);
                        var timeBitmap =
                            WindowOperate.GetItemTimeScreenshot(purpleIMaxLoc.X, i + purpleIMaxLoc.Y, fourStar.Rows);
                        var nameTime = TesseractOperate.GetNameAndTime(nameBitmap, timeBitmap);

                        resList.Add(new HistoryItem(nameTime.name, nameTime.time, type));
                        continue;
                    }
                }

                if (maxBlueVal > threshold && i < blueRes.Rows - threeStar.Rows) // 当前未超出范围且本页有蓝色物品
                {
                    var blueIRect = new Rect(0, i, blueRes.Cols, threeStar.Rows);
                    var blueIRes = new Mat(blueRes, blueIRect);
                    Cv2.MinMaxLoc(blueIRes, out _,  out var blueIMaxVal, out _, out Point blueIMaxLoc);

                    if (blueIMaxVal < threshold) continue;
                    // 当前范围内是蓝色物品
                    var nameBitmap =
                        WindowOperate.GetCharacterNameScreenshot(blueIMaxLoc.X, i + blueIMaxLoc.Y, threeStar.Rows);
                    var timeBitmap =
                        WindowOperate.GetItemTimeScreenshot(blueIMaxLoc.X, i + blueIMaxLoc.Y, threeStar.Rows);
                    var nameTime = TesseractOperate.GetNameAndTime(nameBitmap, timeBitmap);

                    resList.Add(new HistoryItem(nameTime.name, nameTime.time, type, 3));
                }
            }

            return resList;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}