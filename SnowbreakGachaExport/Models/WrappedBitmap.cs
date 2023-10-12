using System.Drawing;

namespace SnowbreakGachaExport.Models;

public class WrappedBitmap : IFullImage
{
    private readonly AppConfig _config;
    private bool _disposed;

    public Bitmap Bitmap { get; private set; }
    public Rectangle Bounds { get; private set; }

    public WrappedBitmap(Bitmap bitmap, AppConfig config, Rectangle bounds)
    {
        Bitmap = bitmap;
        Bounds = bounds;
        _config = config;
    }

    public void Dispose()
    {
        if (_disposed) return;

        Bitmap.Dispose();
        _disposed = true;
    }
}
