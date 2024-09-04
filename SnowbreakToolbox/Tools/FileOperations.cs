using System.IO;
using System.Security.Cryptography;

namespace SnowbreakToolbox.Tools;

public static class FileOperations
{
    public static bool AreFilesEqual(string path1, string path2)
    {
        using var md5 = MD5.Create();
        
        using var stream1 = File.OpenRead(path1);
        using var stream2 = File.OpenRead(path2);
        var hash1 = md5.ComputeHash(stream1);
        var hash2 = md5.ComputeHash(stream2);
        
        return hash1.SequenceEqual(hash2);
    }
}