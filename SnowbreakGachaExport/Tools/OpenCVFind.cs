using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SnowbreakGachaExport.Models;
using Point = OpenCvSharp.Point;

namespace SnowbreakGachaExport.Tools;

public class OpenCVFind
{
    private const string nextPageArrowImagePath = "./Images/NextPageArrow.png";
    private const string fiveStarImagePath = "./Images/5Star_Big.png";
    private const string fourStarImagePath = "./Images/4Star_Big.png";
    private const string threeStarImagePath = "./Images/3Star_Big.png";
    private static Mat? lastPage = null;

    public static Point FindNextPageArrow()
    {
        try
        {
            var arrowMat =
                new Mat(nextPageArrowImagePath, ImreadModes.AnyColor).CvtColor(ColorConversionCodes.RGBA2RGB);
            var screenShot = WindowOperate.GetScreenShot().ToMat().CvtColor(ColorConversionCodes.RGBA2RGB);
            var result = new Mat();

            Cv2.MatchTemplate(screenShot, arrowMat, result, TemplateMatchModes.CCoeffNormed);

            double minVal;
            double maxVal;
            var minLoc = new Point(0, 0);
            var maxLoc = new Point(0, 0);
            Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);
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

            Console.WriteLine("5:");
            var list = FindItem(fiveStarMat, screenShot);
            var resList = list.Select(item => new HistoryItem(item.Item1, item.Item2, itemType, 5)).ToList();

            Console.WriteLine("4:");
            list = FindItem(fourStarMat, screenShot);
            resList.AddRange(list.Select(item => new HistoryItem(item.Item1, item.Item2, itemType, 4)).ToList());

            Console.WriteLine("3:");
            list = FindItem(threeStarMat, screenShot);
            resList.AddRange(list.Select(item => new HistoryItem(item.Item1, item.Item2, star:3)).ToList());

            return resList;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static IEnumerable<(string?, string?)> FindItem(Mat starImage, Mat screenshot)
    {
        try
        {
            var list = new List<(string? name, string? time)>(); // List to return

            var res = new Mat();
            double minVal;
            double maxVal;
            Point minLoc = new(0, 0);
            Point maxLoc = new(0, 0);
            const double threshold = 0.95;

            Cv2.MatchTemplate(screenshot, starImage, res, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(res, out minVal, out maxVal, out minLoc, out maxLoc);
            if (maxVal < threshold) return list;

            Cv2.Normalize(res, res, 0, 1, NormTypes.MinMax, -1);

            for (var i = 1; i < res.Rows - starImage.Rows; i += starImage.Rows)
            {
                for (var j = 1; j < res.Cols - starImage.Cols; j += starImage.Cols)
                {
                    Rect rij = new(j, i, starImage.Cols, starImage.Rows);
                    Mat rijRes = new(res, rij);
                    Cv2.MinMaxLoc(rijRes, out minVal, out maxVal, out minLoc, out maxLoc);
                    if (maxVal > threshold)
                    {
                        var bitmap =
                            WindowOperate.GetCharacterNameScreenshot(j + maxLoc.X, i + maxLoc.Y, starImage.Rows);
                        var timeBitMap =
                            WindowOperate.GetItemTimeScreenshot(j + maxLoc.X, i + maxLoc.Y, starImage.Rows);
                        list.Add((TesseractOperate.GetTextFromBitmap(bitmap),
                            TesseractOperate.GetIntegerFromBitmap(timeBitMap)));
                    }
                }
            }

            return list;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error in FIndItem" + e);
            throw;
        }
    }
}