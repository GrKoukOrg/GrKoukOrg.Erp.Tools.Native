using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class ItemBuyDocListPageModel : ObservableObject, IQueryAttributable
{
    private readonly LocalBuyDocLinesRepo _localBuyDocLinesRepo;
    private readonly LocalBuyDocumentsRepo _localBuyDocumentsRepo;
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<ItemBuyDocListPageModel> _logger;
    private int _itemId;
    [ObservableProperty] private ICollection<ItemBuyListLineDto> _items;
    [ObservableProperty] private DateTime _fromDate;
    [ObservableProperty] private DateTime _toDate;

    public ItemBuyDocListPageModel(LocalBuyDocLinesRepo localBuyDocLinesRepo,
        LocalBuyDocumentsRepo localBuyDocumentsRepo, ModalErrorHandler errorHandler,ILogger<ItemBuyDocListPageModel> logger)
    {
        _localBuyDocLinesRepo = localBuyDocLinesRepo;
        _localBuyDocumentsRepo = localBuyDocumentsRepo;
        _errorHandler = errorHandler;
        _logger = logger;
    }

    [RelayCommand]
    private async Task Appearing()
    {
      _logger.LogDebug("Inside Appearing");
        //Items = await _localBuyDocLinesRepo.ListBuyDocLinesByDateRangeAsync(FromDate,ToDate);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _logger.LogDebug("Inside ApplyQueryAttributes");
        ToDate = DateTime.Today;
        FromDate = DateTime.Today.AddDays(-30);
        if (query.ContainsKey("itemid"))
        {
            _itemId = Convert.ToInt32(query["itemid"]);
            LoadData(_itemId).FireAndForgetSafeAsync(_errorHandler);
        }
    }

    [RelayCommand]
    private async Task DateSelected(DateTime selectedDate)
    {
        try
        {
            await LoadData(_itemId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in DateSelected");
            await AppShell.DisplayToastAsync($"Error in DateSelected: {e.Message}");
        }
    }
    private async Task LoadData(int itemId)
    {
        _logger.LogDebug("Inside LoadData");
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
                LineDiscountAmount = l.LineDiscountAmount,
                UnitDiscountRate = l.UnitDiscountRate,
                LineNetAmount = l.LineNetAmount,
                LineTotalAmount = l.LineTotalAmount,
                LineVatAmount = l.LineVatAmount,
                BuyDocDefName = d.BuyDocDefName,
                UnitOfMeasureName = l.UnitOfMeasureName
            }
        ).ToList();
    }
}