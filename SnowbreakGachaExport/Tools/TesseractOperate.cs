using System;
using System.Drawing;
using System.Text;
using Tesseract;

namespace SnowbreakGachaExport.Tools;

public static class TesseractOperate
{
    public static string? GetTextFromBitmap(Bitmap bitmap)
    {
        try
        {
            using var engine = new TesseractEngine(@"./TessData", "chi_sim+eng+jpn", EngineMode.Default);
            using var page = engine.Process(bitmap);
            StringBuilder text = new(page.GetText());
            text = text.Replace("|", "").Replace(" ", "").Replace("\r\n", "")
                .Replace("\r", "").Replace("\n", "").Replace("景-", "晴-")
                .Replace("翠帯", "绷带").Replace("②", "2").Replace("①", "").Replace("⑥", "6")
                .Replace("③", "3").Replace("④", "4").Replace("⑤", "5").Replace("⑦", "7")
                .Replace("⑧", "8").Replace("⑨", "9");
            Console.WriteLine(text);

            return text.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw; 
        }
    }

    public static string? GetIntegerFromBitmap(Bitmap bitmap, string whiteList = "0123456789")
    {
        try
        {
            using var engine = new TesseractEngine(@"./TessData", "eng", EngineMode.Default);
            engine.SetVariable("tessedit_char_whitelist", whiteList);
            using var page = engine.Process(bitmap);
            StringBuilder text = new(page.GetText());
            text = text.Replace("|", "").Replace(" ", "").Replace("\r\n", "")
                .Replace("\r", "").Replace("\n", "");
            Console.WriteLine(text);

            return text.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw; 
        }
    }
}