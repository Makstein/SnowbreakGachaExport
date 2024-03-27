using System.Windows.Media;

namespace SnowbreakToolbox.Models;

public partial class TetrisMapCell : ObservableObject
{
    public byte Value { get; set; }
    
    [ObservableProperty]
    private Brush _cellColor = new SolidColorBrush(Colors.Azure);
    
    [RelayCommand]
    private void OnChangeCellStatus()
    {
        if (Value == 0)
        {
            Value = 0xff;
            CellColor = new SolidColorBrush(Colors.Gray);
        }
        else
        {
            Value = 0;
            CellColor = new SolidColorBrush(Colors.Azure);
        }
    }
}