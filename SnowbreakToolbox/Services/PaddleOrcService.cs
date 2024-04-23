using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;
using SnowbreakToolbox.Interfaces;
using System.Diagnostics;
using System.Drawing;

namespace SnowbreakToolbox.Services;

/// <summary>
/// Class for ocr
/// </summary>
public class PaddleOrcService : ISnowbreakOcr
{
    private PaddleOcrAll? _all;
    private bool _initialized;

    public PaddleOrcService()
    {
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await Task.Run(() =>
        {
            _all = new PaddleOcrAll(LocalFullModels.ChineseV4, PaddleDevice.Onnx());
            _initialized = true;
        });
    }

    private void EnsureModelLoaded()
    {
        var sp = Stopwatch.StartNew();

        while (_all == null || !_initialized)
        {
            Task.Delay(100).Wait();
            if (sp.Elapsed.Seconds > 10)
            {
                throw new Exception("Failed to load ocr model");
            }
        }
    }

    private void GetText(Mat image)
    {
        EnsureModelLoaded();

        var result = _all!.Run(image);
        var regions = result.Regions;
        regions = [.. regions.OrderBy(item => item.Rect.Points()[0].Y)];

#if DEBUG
        foreach (PaddleOcrResultRegion region in regions)
        {
            Debug.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectX: {region.Rect.Points()[0]}, RectSize: {region.Rect.Size}, Angle: {region.Rect.Angle}");
        }
#endif

        for (var i = 0; i < regions.Length; i += 3)
        {
            PaddleOcrResultRegion[] singleLog = [regions[i], regions[i + 1], regions[i + 2]];
            singleLog = [.. singleLog.OrderBy(item => item.Rect.Points()[0].X)];
            Debug.WriteLine($"Name: {singleLog[0].Text}, Type: {singleLog[1].Text}, Time: {singleLog[2].Text}");
        }
    }

    private List<PaddleOcrResultRegion[]> GetRegions(Mat image)
    {
        EnsureModelLoaded();

        var res = new List<PaddleOcrResultRegion[]>();

        var result = _all!.Run(image);
        var regions = result.Regions;

#if DEBUG
        foreach (PaddleOcrResultRegion region in regions)
        {
            Debug.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectX: {region.Rect.Points()[0]}, RectSize: {region.Rect.Size}, Angle: {region.Rect.Angle}");
        }
#endif

        if (regions.Length % 3 != 0)
        {
            throw new Exception("Length of region array incorrect");
        }

        regions = [.. regions.OrderBy(item => item.Rect.Points()[0].Y)];
        for (var i = 0; i < regions.Length; i += 3)
        {
            PaddleOcrResultRegion[] singleLog = [regions[i], regions[i + 1], regions[i + 2]];
            singleLog = [.. singleLog.OrderBy(item => item.Rect.Points()[0].X)];
            res.Add(singleLog);
        }

        return res;
    }

    public List<PaddleOcrResultRegion[]> GetRegions(Bitmap image) => GetRegions(image.ToMat());

    public void GetText(Bitmap image) => GetText(image.ToMat());
}
