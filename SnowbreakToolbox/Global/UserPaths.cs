using System.IO;

namespace SnowbreakToolbox.Global;

public static class UserPaths
{
    public static readonly string BasePath = AppContext.BaseDirectory;
    public static readonly string ConfPath = Path.Combine(BasePath, "Config");
    public static readonly string DataPath = Path.Combine(BasePath, "Data");
    public static readonly string ConfFile = Path.Combine(ConfPath, "GeneralConfig.json");
    public static readonly string DataFile = Path.Combine(DataPath, "HistoryCache.json");
}
