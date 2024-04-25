using System.Drawing;
using System.Drawing.Imaging;
using Vanara.PInvoke;

namespace SnowbreakToolbox.Tools;

public class ScreenOperations
{
    public static Bitmap CaptureRegion(int X0, int Y0, int width, int height)
    {
        var image = new Bitmap(width, height, PixelFormat.Format32bppRgb);
        using Graphics g = Graphics.FromImage(image);

        g.CopyFromScreen(X0, Y0, 0, 0, new System.Drawing.Size(width, height));

#if DEBUG
        image.Save("test.png");
#endif

        return image;
    }
}
