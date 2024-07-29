using SnowbreakToolbox.Models;
using SnowbreakToolbox.Tools;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class TetrisViewModel : ObservableObject
{
    public ObservableCollection<TetrisMapCell> Cells { get; set; } = [];
    public List<byte> BlockCount { get; set; } = [];
    public ObservableCollection<TetrisBlockProperty> Blocks { get; set; } = [];

    private int _curSolutionIndex;
    private List<IList<byte[]>>? _solutions;

    private readonly List<Brush> _blockColors =
        [
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#97b6d0")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9596d6")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#93b6d2")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94b9bb")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bbc896")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99c395")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cbba9c")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c6c192")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#cfa6c6")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f7babb")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a2abcb")),
        ];

    public TetrisViewModel()
    {
        // Initialize puzzle map
        for (var i = 0; i < 30; i++)
        {
            Cells.Add(new TetrisMapCell());
        }

        // Initialize tetris block image and count
        for (int i = 1; i <= 11; i++)
        {
            Blocks.Add(new TetrisBlockProperty($"pack://application:,,,/Assets/tetris{i}.png", 0));
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

        var mustUseBlocks = new List<byte>();
        for (byte i = 0; i < Blocks.Count; i++)
        {
            if (Blocks[i].MustUse)
            {
                mustUseBlocks.Add((byte)(i + 1));
            }
        }
        _solutions = _solutions.Where((x) =>
        {
            foreach (var blockIndex in mustUseBlocks)
            {
                var singleBlockRes = false;
                foreach (var row in x)
                {
                    if (row.Contains(blockIndex))
                    {
                        singleBlockRes = true;
                        break;
                    }
                }
                if (!singleBlockRes)
                    return false;
            }
            return true;
        }).ToList();

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
        var solution = _solutions[_curSolutionIndex];

        for (var i = 0; i < solution.Count; i++)
        {
            for (var j = 0; j < solution[i].Length; j++)
            {
                if (solution[i][j] == 0xff) continue;

                Cells[i * 6 + j].CellColor = _blockColors[solution[i][j] - 1];
                Cells[i * 6 + j].CellSerialNum = $"{solution[i][j]}";
            }
        }
    }
}
