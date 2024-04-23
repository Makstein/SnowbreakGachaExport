using System.Drawing;
using System.Numerics;

namespace SnowbreakToolbox.Tools;

public static class ImageOperations
{
    public static double ImageMse(Bitmap bmp1, Bitmap bmp2)
    {
        double mes = 0;

        var rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);

        var data1 = bmp1.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var data2 = bmp2.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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

        bmp1.UnlockBits(data1);
        bmp2.UnlockBits(data2);

        double mesPerPixel = mes / (rect.Width * rect.Height * 3);
        return mesPerPixel;
    }
}
