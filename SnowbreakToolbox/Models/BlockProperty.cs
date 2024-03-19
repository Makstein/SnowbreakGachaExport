using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.Models;

public class BlockProperty(string imageUrl, byte count)
{
    public ImageIcon Icon { get; set; } = new()
    {
        Source = new BitmapImage(new Uri(imageUrl)),
        Height = 32,
        Width = 32,
    };
    public byte BlockCount { get; set; } = count;
}
