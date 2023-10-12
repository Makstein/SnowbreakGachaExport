using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;

namespace SnowbreakGachaExport.Models
{
    public class BitMapPool
    {
        private readonly AppConfig _config;
        private readonly ConcurrentDictionary<Size, ConcurrentBag<WrappedBitmap>> bitmaps = new();
        private readonly ConcurrentBag<WrappedBitmap> allocated = new();

        public BitMapPool(AppConfig config)
        {
            _config = config;
        }

        public PooledWrappedBitmap Rent(Rectangle bitmapRect)
        {
            var bag = bitmaps.GetOrAdd(bitmapRect.Size, _ => new ConcurrentBag<WrappedBitmap>());
            if (!bag.TryTake(out var result))
            {
                result = new WrappedBitmap(new Bitmap(bitmapRect.Width, bitmapRect.Height, PixelFormat.Format32bppArgb),
                    _config, bitmapRect);
                allocated.Add(result);
            }
            return new PooledWrappedBitmap(result, this);
        }

        public void Return(WrappedBitmap bitmap)
        {
            var bag = bitmaps.GetOrAdd(bitmap.Bitmap.Size, _ => new ConcurrentBag<WrappedBitmap>());
            bag.Add(bitmap);
        }

        public void ReleaseAll()
        {
            foreach (var bitmap in allocated)
            {
                bitmap.Dispose();
            }
            allocated.Clear();
            bitmaps.Clear();
        }
    }
}
