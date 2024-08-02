using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowbreakToolbox.Tools;

public class ModPakInfo
{
    
}

/// <summary>
/// Class for read mod pak file, Dev caution: integer in pak file is stored in little-endian
/// </summary>
public static class PakOperations
{
    // All ue pak has this magic number
    private static readonly int _magic = 0x5A6F12E1;
    private static readonly int _magicOffsetVersion10 = 204;

    public static void ReadPakFromPath(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(-_magicOffsetVersion10, SeekOrigin.End);
            using var reader = new BinaryReader(fs, Encoding.ASCII, false);

            // Pak info
            var pakInfoBytes = reader.ReadBytes(_magicOffsetVersion10);
            var magic = reader.ReadInt32();
            if (magic != _magic)
            {
                // if magic error, may be version incorrect, should change _magicOffsetVersionX
                throw new Exception("Error when read pak, magic error");
            }
            var version = reader.ReadInt32();
            var indexOffset = reader.ReadInt64();
            var indexLength = reader.ReadInt64();

            // Pak index
            reader.BaseStream.Seek(indexOffset, SeekOrigin.Begin);
            var mountPointSize = reader.ReadInt32();
            var mountPointBytes = reader.ReadBytes(mountPointSize);
            var mountPoint = Encoding.ASCII.GetString(mountPointBytes);
        }
        catch
        {

        }
    }
}
