using SnowbreakToolbox.Models;

namespace SnowbreakToolbox.Interfaces;

public interface IModService
{
    public ModConfig GetModConfig();
    public Task<ModConfig> GetModConfigAsync();
    public void Save();
}