using SnowbreakGachaExport.Models;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Tesseract;

namespace SnowbreakGachaExport.Tools;

public static class TesseractOperate
{
    public static (string name, string time) GetNameAndTime(Bitmap nameBitmap, Bitmap timeBitmap)
    {
        try
        {
            using var engine = new TesseractEngine("./TessData", "chi_sim+eng+jpn", EngineMode.Default);
            StringBuilder nameText;
            StringBuilder timeText;
            using (var namePage = engine.Process(nameBitmap))
            {
                nameText = new StringBuilder(namePage.GetText());
                nameText = nameText.Replace("|", "").Replace(" ", "").Replace("\r\n", "")
                    .Replace("\r", "").Replace("\n", "").Replace("景-", "晴-")
                    .Replace("翠帯", "绷带").Replace("②", "2").Replace("①", "").Replace("⑥", "6")
                    .Replace("③", "3").Replace("④", "4").Replace("⑤", "5").Replace("⑦", "7")
                    .Replace("⑧", "8").Replace("⑨", "9");
                Console.WriteLine(nameText);
            }
            engine.SetVariable("tessedit_char_whitelist", "0123456789");
            using (var timePage = engine.Process(timeBitmap))
            {
                timeText = new StringBuilder(timePage.GetText());
                timeText = timeText.Replace("|", "").Replace(" ", "").Replace("\r\n", "")
                    .Replace("\r", "").Replace("\n", "");
            }
            return (nameText.ToString(), timeText.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static (ImmutableArray<string>, ImmutableArray<string>) GetNameAndTime
        (ImmutableArray<PooledWrappedBitmap> nameBitmaps, ImmutableArray<PooledWrappedBitmap> timeBitmaps)
    {
        var names = ImmutableArray.CreateBuilder<string>(nameBitmaps.Length);
        var times = ImmutableArray.CreateBuilder<string>(timeBitmaps.Length);

        using var engine = new TesseractEngine("./TessData", "chi_sim", EngineMode.Default);

        foreach (var bitmap in nameBitmaps)
        {
            using var namePage = engine.Process(bitmap.Inner.Bitmap);
            
            var nameText = new StringBuilder(namePage.GetText());
            nameText = nameText.Replace("|", "").Replace(" ", "").Replace("\r\n", "")
                    .Replace("\r", "").Replace("\n", "").Replace("景-", "晴-")
                    .Replace("翠帯", "绷带").Replace("②", "2").Replace("①", "").Replace("⑥", "6")
                    .Replace("③", "3").Replace("④", "4").Replace("⑤", "5").Replace("⑦", "7")
                    .Replace("⑧", "8").Replace("⑨", "9");

            names.Add(nameText.ToString());

            Debug.WriteLine(nameText);
        }

        engine.SetVariable("tessedit_char_whitelist", "0123456789");

        foreach (var bitmap in timeBitmaps)
        {
            using var timePage = engine.Process(bitmap.Inner.Bitmap);

            var timeText = new StringBuilder(timePage.GetText());
            timeText = timeText.Replace("|", "").Replace(" ", "").Replace("\r\n", "")
                    .Replace("\r", "").Replace("\n", "");

            times.Add(timeText.ToString());

            Debug.WriteLine(timeText);
        }

        return (names.ToImmutable(), times.ToImmutable());
    }
}