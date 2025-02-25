using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class ErpCashDiaryListPageModel:ObservableObject
{
    private readonly ILocalCashDiaryRepo<CashDiaryItemDto> _localCashDiaryRepo;

    public ErpCashDiaryListPageModel(ILocalCashDiaryRepo<CashDiaryItemDto> localCashDiaryRepo)
    {
        _localCashDiaryRepo = localCashDiaryRepo;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        
    }
}