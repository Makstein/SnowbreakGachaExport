using SnowbreakToolbox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowbreakToolbox.Interfaces;

public interface ISnowbreakConfig
{
    AppConfig GetConfig();
    void SetConfig(AppConfig config);
    void Save();
}
