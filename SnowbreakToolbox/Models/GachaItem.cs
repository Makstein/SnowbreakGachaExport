using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace SnowbreakToolbox.Models
{
    public enum ItemType
    {
        Character,
        Weapon
    }
    public class GachaItem
    {
        public GachaItem(string name = "", string time = "", ItemType type = ItemType.Weapon, int star = 4)
        {
            Name = name;
            Type = type;
            Star = star;
            Time = time;
            Id = name + time;
        }

        [JsonConstructor]
        public GachaItem(string id, string name = "", string time = "", ItemType type = ItemType.Weapon, int star = 4)
        {
            Id = id;
            Name = name;
            Type = type;
            Star = star;
            Time = time;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public int Star { get; set; }
        public string Time { get; set; }
    }
}
