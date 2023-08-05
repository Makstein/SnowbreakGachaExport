using System;
using System.Drawing;
using Tesseract;

namespace SnowbreakGachaExport.Tools;

public class TesseractOperate
{
    public static string? GetTextFromBitmap(Bitmap bitmap)
    {
        try
        {
            string? text;
            using (var engine = new TesseractEngine(@"./TessData", "chi_sim", EngineMode.Default))
            {
                using (var page = engine.Process(bitmap))
                {
                    text = page.GetText();
                    text = text.Replace("|", "").Replace(" ", "").Replace("\r\n", "")
                        .Replace("\r", "").Replace("\n", "").Replace("景-", "晴-");
                    Console.WriteLine(text);
                }
            }

            return text;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw; 
        }
    }
}