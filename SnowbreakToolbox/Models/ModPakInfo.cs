using System;
using System.Collections.Generic;
using System.IO;
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
    public string ModPath { get; set; } = string.Empty;

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;
            _isEnabled = value;
            var newPath = Path.ChangeExtension(ModPath, _isEnabled ? "pak" : "disabled");
            if (newPath.Equals(ModPath, StringComparison.Ordinal)) return;
            File.Move(ModPath, newPath);
            ModPath = newPath;
        }
    }
}