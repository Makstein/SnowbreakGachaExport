using SnowbreakToolbox.Models;
using System.IO;
using System.Text;

namespace SnowbreakToolbox.Tools;

/// <summary>
/// Class for read mod pak file, Dev caution: integer in pak file is stored in little-endian
/// </summary>
public static class PakOperations
{
    // All ue pak has this magic number
    private static readonly int _magic = 0x5A6F12E1;

    private static readonly int _magicOffsetVersion10 = 204;
    private static readonly string _characterPrefix = "girl";

    /// <summary>
    /// For research pak file only
    /// May use less ram than Brute if pak file too large
    /// Otherwise, Brute is more reliable
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static ModPakInfo ReadPakFromPathUnpack(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        fs.Seek(-_magicOffsetVersion10, SeekOrigin.End);
        using var reader = new BinaryReader(fs, Encoding.ASCII, false);

        // Pak info
        var magic = reader.ReadInt32();
        if (magic != _magic)
        {
            // if magic error, may be version incorrect, should change _magicOffsetVersionX
            throw new Exception("Read pak error, magic error");
        }
        var version = reader.ReadInt32();
        var indexOffset = reader.ReadInt64();
        var indexLength = reader.ReadUInt64();

        // Pak index
        reader.BaseStream.Seek(indexOffset, SeekOrigin.Begin);
        var mountPointSize = reader.ReadInt32();
        var mountPointBytes = reader.ReadBytes(mountPointSize);
        var mountPoint = Encoding.ASCII.GetString(mountPointBytes);

        // If pak only contain character
        if (mountPoint.Contains(_characterPrefix))
        {
            var modPakInfo = GetModPakInfoFromDirectoryName(mountPoint);
            return modPakInfo;
        }

        var entryCount = reader.ReadUInt32();       // not use
        var pathHasSeed = reader.ReadBytes(8);      // not use
        var pathHasHashIndex = reader.ReadUInt32(); // not use
        if (pathHasHashIndex != 0)
        {
            var pathHashIndexOffset = reader.ReadInt64();
            var pathHashIndexSize = reader.ReadInt64();
            var pathHashIndexHash = reader.ReadBytes(20);
        }

        var hasFullDirectoryIndex = reader.ReadUInt32();
        if (hasFullDirectoryIndex == 0)
        {
            throw new Exception("Read pak error, not have full directory index");
        }

        var fullDirectoryIndexOffset = reader.ReadInt64();
        reader.BaseStream.Seek(fullDirectoryIndexOffset, SeekOrigin.Begin);

        var directoryCount = reader.ReadUInt32();
        for (int i = 0; i < directoryCount; i++)
        {
            var directoryNameSize = reader.ReadInt32();
            var directoryNameBytes = reader.ReadBytes(directoryNameSize);
            var directoryName = Encoding.ASCII.GetString(directoryNameBytes, 0, directoryNameSize - 1);

            if (!directoryName.Contains(_characterPrefix))
            {
                var fileCount = reader.ReadUInt32();
                if (fileCount != 0)
                {
                    throw new Exception("Read pak error, unexpected directory order");
                }
                continue;
            }

            return GetModPakInfoFromDirectoryName(directoryName);
        }

        return new ModPakInfo();
    }

    public static ModPakInfo ReadPakFromPathBrute(string path)
    {
        var str = File.ReadAllText(path);
        return GetModPakInfoFromDirectoryName(str);
    }

    private static ModPakInfo GetModPakInfoFromDirectoryName(string str)
    {
        var modPakInfo = new ModPakInfo();
        var index = str.IndexOf(_characterPrefix);
        if (str[index + 7] == 'a') // 5-star
        {
            modPakInfo.CharacterCode = str.Substring(index, 8);
            if (str[index + 8] == '_')
            {
                modPakInfo.SkinIndex = int.Parse(str.Substring(index + 9, 2));
            }
        }
        else // 4-star
        {
            modPakInfo.CharacterCode = str.Substring(index, 7);
            if (str[index + 7] == '_')
            {
                modPakInfo.SkinIndex = int.Parse(str.Substring(index + 8, 2));
            }
        }

        return modPakInfo;
    }
}
