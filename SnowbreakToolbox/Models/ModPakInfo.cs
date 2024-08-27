using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowbreakToolbox.Models;

public class ModPakInfo
{
    public string Name { get; set; } = string.Empty;
    public string CharacterCode { get; set; } = string.Empty;
    public int SkinIndex { get; set; } = 0;                         // 0: Origin 1: Dormitory version 2: Skin 1
    public string Description { get; set; } = "No Description";
    public bool IsEnabled { get; set; } = false;
    public string ModPath { get; set; } = string.Empty;
}