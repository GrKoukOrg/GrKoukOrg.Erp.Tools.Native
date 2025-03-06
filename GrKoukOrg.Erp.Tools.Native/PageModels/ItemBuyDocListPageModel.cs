using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class ItemBuyDocListPageModel : ObservableObject, IQueryAttributable
{
    private readonly LocalBuyDocLinesRepo _localBuyDocLinesRepo;
    private readonly LocalBuyDocumentsRepo _localBuyDocumentsRepo;
    private readonly ModalErrorHandler _errorHandler;
    private int _itemId;
    [ObservableProperty] private ICollection<ItemBuyListLineDto> _items;
    [ObservableProperty] private DateTime _fromDate;
    [ObservableProperty] private DateTime _toDate;

    public ItemBuyDocListPageModel(LocalBuyDocLinesRepo localBuyDocLinesRepo,
        LocalBuyDocumentsRepo localBuyDocumentsRepo, ModalErrorHandler errorHandler)
    {
        _localBuyDocLinesRepo = localBuyDocLinesRepo;
        _localBuyDocumentsRepo = localBuyDocumentsRepo;
        _errorHandler = errorHandler;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        ToDate = DateTime.Today;
        FromDate = DateTime.Today.AddDays(-30);
        //Items = await _localBuyDocLinesRepo.ListBuyDocLinesByDateRangeAsync(FromDate,ToDate);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("itemid"))
        {
            _itemId = Convert.ToInt32(query["itemid"]);
            LoadData(_itemId).FireAndForgetSafeAsync(_errorHandler);
        }
    }

    [RelayCommand]
    public async void DateSelected(DateTime selectedDate)
    {
        LoadData(_itemId).FireAndForgetSafeAsync(_errorHandler);
    }
    private async Task LoadData(int itemId)
    {
        var buyDocLines = await _localBuyDocLinesRepo.ListBuyDocLinesByDateRangeAsync(itemId, FromDate, ToDate);
        var buyDocs = await _localBuyDocumentsRepo.ListBuyDocsAsync();
        Items = buyDocLines.Join(
            buyDocs,
            l => l.BuyDocId,
            d => d.Id,
            (l, d) => new ItemBuyListLineDto()
            {
                ItemId = l.ItemId,
                ItemCode = l.ItemCode,
                ItemName = l.ItemName,
                RefNumber = d.RefNumber,
                SupplierId = d.SupplierId,
                SupplierName = d.SupplierName,
                TransDate = d.TransDate,
                UnitPrice = l.UnitPrice,
                UnitQty = l.UnitQty,
                UnitDiscountAmount = l.UnitDiscountAmount,
                UnitDiscountRate = l.UnitDiscountRate,
                UnitNetAmount = l.UnitNetAmount,
                UnitTotalAmount = l.UnitTotalAmount,
                UnitVatAmount = l.UnitVatAmount,
                BuyDocDefName = d.BuyDocDefName,
                UnitOfMeasureName = l.UnitOfMeasureName
            }
        ).ToList();
    }
}