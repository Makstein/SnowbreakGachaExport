using SnowbreakToolbox.Interfaces;
using SnowbreakToolbox.Services;

namespace SnowbreakToolbox.ViewModels.Pages;

public partial class GachaHistoryViewModel(ISnowbreakOcr snowbreakOcr) : ObservableObject
{
    private readonly PaddleOrcService paddleOrcService = (snowbreakOcr as PaddleOrcService)!;

    [RelayCommand]
    private void OnGetHistory()
    {

    }
}
