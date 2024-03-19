using SnowbreakToolbox.Models;
using SnowbreakToolbox.Tools;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class TetrisViewModel : ObservableObject
{
    public ObservableCollection<TetrisMapCell> Cells { get; set; } = [];
    public List<byte> BlockCount { get; set; } = [];
    public ObservableCollection<BlockProperty> Blocks { get; set; } = [];

    private int _curSolutionIndex;
    private List<IList<byte[]>>? _solutions;

    private readonly List<Brush> _blockColors
        = [new SolidColorBrush(Colors.LightSkyBlue),
            new SolidColorBrush(Colors.MediumPurple),
            new SolidColorBrush(Colors.DodgerBlue),
            new SolidColorBrush(Colors.YellowGreen),
            new SolidColorBrush(Colors.GreenYellow),
            new SolidColorBrush(Colors.Yellow),
            new SolidColorBrush(Colors.Brown),
            new SolidColorBrush(Colors.Gold),
            new SolidColorBrush(Colors.OrangeRed),
            new SolidColorBrush(Colors.IndianRed),
            new SolidColorBrush(Colors.MediumVioletRed),
            new SolidColorBrush(Colors.Black)];

    public TetrisViewModel()
    {
        for (var i = 0; i < 30; i++)
        {
            Cells.Add(new TetrisMapCell());
        }

        for (int i = 1; i <= 11; i++)
        {
            Blocks.Add(new BlockProperty($"pack://application:,,,/Assets/tetris1.png", 99));
        }
    }

    [RelayCommand]
    private void OnGetSolution()
    {
        var map = new List<byte[]>();
        for (var i = 0; i < Cells.Count; i += 6)
        {
            var bytes = new byte[6];
            for (int j = 0; j < 6; j++)
                bytes[j] = Cells[i + j].Value;
            map.Add(bytes);
        }
        BlockCount.Clear();
        foreach (var i in Blocks)
        {
            BlockCount.Add(i.BlockCount);
        }

        _solutions = Tetris.GetSolutions(map, [.. BlockCount]);
        _curSolutionIndex = 0;
        OnChangeDisplaySolution("0");
    }

    [RelayCommand]
    private void OnChangeDisplaySolution(string str)
    {
        if (_solutions == null)
        {
            return;
        }

        int param = int.Parse(str);

        if ((_curSolutionIndex + param > _solutions.Count - 1) || (_curSolutionIndex + param < 0))
        {
            return;
        }

        _curSolutionIndex += param;
        Debug.WriteLine(_curSolutionIndex + " " + _solutions.Count);
        var solution = _solutions[_curSolutionIndex];
        for (var i = 0; i < solution.Count; i++)
        {
            for (var j = 0; j < solution[i].Length; j++)
            {
                Debug.Write(solution[i][j].ToString() + " ");
            }
            Debug.WriteLine("");
        }


        for (var i = 0; i < solution.Count; i++)
        {
            for (var j = 0; j < solution[i].Length; j++)
            {
                if (solution[i][j] == 0xff) continue;

                Cells[i * 6 + j].CellColor = _blockColors[solution[i][j] - 1];
            }
        }
    }
}
