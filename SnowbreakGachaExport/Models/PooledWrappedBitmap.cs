using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowbreakGachaExport.Models;

public class PooledWrappedBitmap : IFullImage
{
    public WrappedBitmap Inner { get; }
    public BitMapPool Pool { get; }
    public bool Disposed { get; private set; }

    public PooledWrappedBitmap(WrappedBitmap inner, BitMapPool pool)
    {
        Inner = inner;
        Pool = pool;
    }

    public void Dispose()
    {
        if (!Disposed)
        {
            Pool.Return(Inner);
            Disposed = true;
        }
    }
}
