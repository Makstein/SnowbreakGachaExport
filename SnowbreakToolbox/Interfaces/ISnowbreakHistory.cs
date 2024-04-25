using SnowbreakToolbox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowbreakToolbox.Interfaces
{
    public interface ISnowbreakHistory
    {
        public Dictionary<string, List<GachaItem>> GetGachaHistory();

        public void SaveGachaHistory(Dictionary<string, List<GachaItem>> newHistory);
    }
}
