using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class ErpCashDiaryListPageModel : ObservableObject
{
    private readonly ILocalCashDiaryRepo<CashDiaryItemDto> _localCashDiaryRepo;
    [ObservableProperty] private ICollection<CashDiaryItemDto> _items;
    public ObservableCollection<TransNameSuggestion> TransNameSuggestions { get; set; }

    public ErpCashDiaryListPageModel(ILocalCashDiaryRepo<CashDiaryItemDto> localCashDiaryRepo)
    {
        _localCashDiaryRepo = localCashDiaryRepo;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        Items = await _localCashDiaryRepo.ListItemsAsync();

        TransNameSuggestions = new ObservableCollection<TransNameSuggestion>(Items.Select(p => new TransNameSuggestion()
        {
            Id = p.Id,
            TransName = p.TransName
        }));
    }
}

public class TransNameSuggestion
{
    public string TransName { get; set; }
    public int Id { get; set; }
}