namespace SnowbreakToolbox.Tools;

// DFS
public class Tetris
{
    private static readonly List<List<List<byte[]>>> _blocks =
    [
      [
        [
          [1, 1],
          [1, 1]
        ]
      ],
      [
        [
          [2, 2, 2, 2]
        ],
        [
          [2],
          [2],
          [2],
          [2]
        ]
      ],
      [
        [
          [3, 3, 0],
          [0, 3, 3]
        ],
        [
          [0, 3],
          [3, 3],
          [3, 0]
        ]
      ],
      [
        [
          [0, 4, 4],
          [4, 4, 0]
        ],
        [
          [4, 0],
          [4, 4],
          [0, 4]
        ]
      ],
      [
        [
          [5, 0, 0],
          [5, 5, 5]
        ],
        [
          [5, 5],
          [5, 0],
          [5, 0]
        ],
        [
          [5, 5, 5],
          [0, 0, 5]
        ],
        [
          [0, 5],
          [0, 5],
          [5, 5]
        ]
      ],
      [
        [
          [0, 0, 6],
          [6, 6, 6]
        ],
        [
          [6, 6],
          [0, 6],
          [0, 6]
        ],
        [
          [6, 6, 6],
          [6, 0, 0]
        ],
        [
          [6, 0],
          [6, 0],
          [6, 6]
        ]
      ],
      [
        [
          [0, 7, 0],
          [7, 7, 7]
        ],
        [
          [7, 7, 7],
          [0, 7, 0]
        ],
        [
          [7, 0],
          [7, 7],
          [7, 0]
        ],
        [
          [0, 7],
          [7, 7],
          [0, 7]
        ]
      ],
      [
        [
          [0, 8, 0],
          [8, 8, 8],
          [0, 8, 0]
        ]
      ],
      [
        [
          [9]
        ]
      ],
      [
        [
          [10, 10]
        ],
        [
          [10],
          [10]
        ]
      ],
      [
        [
          [11, 11],
          [11, 0],
        ],
        [
          [11, 11],
          [0, 11],
        ],
        [
          [0, 11],
          [11, 11],
        ],
        [
          [11, 0],
          [11, 11],
        ],
      ],
    ];

    private static IList<byte[]>? _map;
    private static byte[]? _blockCount;
    private static int _m;
    private static int _n;
    private static List<IList<byte[]>> _result = [];

    public static List<IList<byte[]>> GetSolutions(IList<byte[]> map, byte[] blockCount)
    {
        _map = map;
        _blockCount = blockCount;
        _m = map.Count;
        _n = map[0].Length;
        _result.Clear();

        dfs(0);

        return _result;
    }

    private static bool dfs(int pos)
    {
        // Find one solution
        if (pos == _m * _n)
        {
            // Generate new solution from current map
            var curRes = new List<byte[]>();
            for (int i = 0; i < _map!.Count; i++)
            {
                var curLine = new List<byte>();
                for (int j = 0; j < _map![i].Length; j++)
                {
                    curLine.Add(_map![i][j]);
                }
                curRes.Add([.. curLine]);
            }

            _result.Add(curRes);

            return _result.Count > 1000;
        }

        var x = pos / _n;
        var y = pos % _n;

        if (_map![x][y] != 0)
        {
            return dfs(pos + 1);
        }

        for (var blockIndex = 0; blockIndex < _blocks.Count; blockIndex++)
        {
            if (_blockCount![blockIndex] == 0)
                continue;

            for (var rotationIndex = 0; rotationIndex < _blocks[blockIndex].Count; rotationIndex++)
            {
                if (!CanPlaceBlock(x, y, blockIndex, rotationIndex))
                    continue;

                PlaceBlock(x, y, blockIndex, rotationIndex, (byte)(blockIndex + 1));
                _blockCount[blockIndex]--;
                if (dfs(pos + 1))
                    return true;
                _blockCount[blockIndex]++;
                PlaceBlock(x, y, blockIndex, rotationIndex, 0);
            }
        }

        return false;
    }

    private static bool CanPlaceBlock(int x, int y, int blockIndex, int rotationIndex)
    {
        var block = _blocks[blockIndex][rotationIndex];

        var offset = 0;
        while (block[0][offset] == 0) offset++;
        y -= offset;
        if (y < 0)
            return false;

        for (var i = 0; i < block.Count; i++)
        {
            for (var j = 0; j < block[i].Length; j++)
            {
                if (block[i][j] != 0 &&(x + i >= _m || y + j >= _n || _map![x + i][y + j] != 0))
                    return false;
            }
        }

        return true;
    }

    private static void PlaceBlock(int x, int y, int blockIndex, int rotationIndex, byte content)
    {
        var block = _blocks[blockIndex][rotationIndex];

        var offset = 0;
        while (block[0][offset] == 0) offset++;
        y -= offset;
        
        for (var i = 0; i < block.Count; i++)
        {
            for (var j = 0; j < block[i].Length; j++)
            {
                if (block[i][j] != 0)
                    _map![x + i][y + j] = content;
            }
        }
    }
}

