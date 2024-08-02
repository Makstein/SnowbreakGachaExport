using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SnowbreakToolbox.Models
{
    public class Mod
    {
        public string Name { get; set; }            = string.Empty;
        public string Description { get; set; }     = string.Empty;
        public bool IsEnabled { get; set; }         = true;
        public Mod() { }
        public Mod(string name, string description, bool isEnabled)
        {
            Name = name;
            Description = description;
            IsEnabled = isEnabled;
        }
        [JsonConstructor]
        public Mod(Mod mod)
        {
            Name = mod.Name;
            Description = mod.Description;
            IsEnabled = mod.IsEnabled;
        }
    }
}
