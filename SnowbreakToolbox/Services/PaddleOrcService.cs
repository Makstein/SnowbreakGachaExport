using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;
using SnowbreakToolbox.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowbreakToolbox.Services;

public class PaddleOrcService : ISnowbreakOcr
{
    private readonly PaddleOcrAll _all;

    public PaddleOrcService()
    {
        _all = new PaddleOcrAll(LocalFullModels.ChineseV4, PaddleDevice.Onnx());
    }

    public void GetText(Mat image)
    {
        var result = _all.Run(image);
        foreach (PaddleOcrResultRegion region in result.Regions)
        {
            Console.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}, Angle: {region.Rect.Angle}");
        }
    }

    public void GetText(Bitmap image) => GetText(image.ToMat());
}
