using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class ItemBuyDocListPageModel:ObservableObject
{
    private readonly LocalBuyDocLinesRepo _localBuyDocLinesRepo;
    private readonly LocalBuyDocumentsRepo _localBuyDocumentsRepo;
    [ObservableProperty] private ICollection<BuyDocLineListDto> _items;
    [ObservableProperty] private DateTime _fromDate;
    [ObservableProperty] private DateTime _toDate;
    
    public ItemBuyDocListPageModel(LocalBuyDocLinesRepo localBuyDocLinesRepo, LocalBuyDocumentsRepo localBuyDocumentsRepo)
    {
        _localBuyDocLinesRepo = localBuyDocLinesRepo;
        _localBuyDocumentsRepo = localBuyDocumentsRepo;
    }
    [RelayCommand]
    private async Task Appearing()
    {
        
        Items = await _localBuyDocLinesRepo.ListBuyDocLinesAsync();


    }
}