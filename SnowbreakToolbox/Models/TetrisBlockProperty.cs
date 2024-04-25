using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace SnowbreakToolbox.Models;

public partial class TetrisBlockProperty(string imageUrl, byte count) : ObservableObject
{
    public ImageIcon Icon { get; set; } = new()
    {
        Source = new BitmapImage(new Uri(imageUrl)),
        Height = 48,
        Width = 48,
    };

    [ObservableProperty]
    private byte _blockCount = count;

    [ObservableProperty]
    private bool _mustUse;

    [RelayCommand]
    private void OnCardSelect()
    {
        MustUse = !MustUse;
    }
}
