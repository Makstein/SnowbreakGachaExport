using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using SnowbreakGachaExport.Models;

namespace SnowbreakGachaExport.ViewModels;

public class PoolLogControlViewModel : ViewModelBase
{
    private int _nowGoldProcess;
    public int NowGoldProcess
    {
        get => _nowGoldProcess;
        set => this.RaiseAndSetIfChanged(ref _nowGoldProcess, value);
    }

    private int _nowPurpleProcess;
    public int NowPurpleProcess
    {
        get => _nowPurpleProcess;
        set => this.RaiseAndSetIfChanged(ref _nowPurpleProcess, value);
    }

    private float _fiveAVG;
    public float FiveAVG
    {
        get => -_fiveAVG;
        set => this.RaiseAndSetIfChanged(ref _fiveAVG, value);
    }
    
    private List<HistoryItem> _logList = new();
    public List<HistoryItem> LogList
    {
        get => _logList;
        set => this.RaiseAndSetIfChanged(ref _logList, value);
    }

    public int MaxGoldProcessValue { get; } = 80;
    
    

    public PoolLogControlViewModel()
    {
        LogList = new List<HistoryItem>();
    }

    public PoolLogControlViewModel(IEnumerable<HistoryItem> logList, int maxProcessValue)
    {
        MaxGoldProcessValue = maxProcessValue;
        
        UpdateList(logList);
    }

    public void UpdateList(IEnumerable<HistoryItem> newList)
    {
        LogList = new List<HistoryItem>(newList);

        var bFIndPurple = true;
        for (var i = _logList.Count - 1; i >= 0; --i)
        {
            switch (_logList[i].Star)
            {
                case 5:
                {
                    NowGoldProcess = _logList.Count - 1 - i;
                    if (bFIndPurple)
                    {
                        NowPurpleProcess = _logList.Count - 1 - i;
                    }
                    return;
                }
                case 4 when bFIndPurple:
                    bFIndPurple = false;
                    NowPurpleProcess = _logList.Count - 1 - i;
                    break;
            }
        }

        NowGoldProcess = _logList.Count;
        if (bFIndPurple)
        {
            NowPurpleProcess = _logList.Count;
        }
    }
}