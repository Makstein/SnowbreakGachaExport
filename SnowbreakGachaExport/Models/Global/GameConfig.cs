using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowbreakGachaExport.Models.Global;

public class GameConfig
{
    public string GameWindowTitle { get; set; }

    public Dictionary<string, string> ReplaceWords { get; set; }
}
