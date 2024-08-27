using SnowbreakToolbox.Models;

namespace SnowbreakToolbox.Interfaces;

public interface IModService
{
    public ModConfig GetModConfig();
    public void Save();
}