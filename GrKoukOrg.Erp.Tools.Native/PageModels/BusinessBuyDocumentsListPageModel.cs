using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class BusinessBuyDocumentsListPageModel : ObservableObject
{
    private readonly ILogger<BusinessBuyDocumentsListPageModel> _logger;
    private readonly INavigationParameterService _navParameterService;
    private readonly ModalErrorHandler _errorHandler;
    [ObservableProperty] private ObservableCollection<BusinessBuyDocUpdateItem> _items;
    [ObservableProperty] private bool _isBusy = false;

    public BusinessBuyDocumentsListPageModel(ILogger<BusinessBuyDocumentsListPageModel> logger
        , INavigationParameterService navParameterService
        , ModalErrorHandler errorHandler)
    {
        _logger = logger;
        _navParameterService = navParameterService;
        _errorHandler = errorHandler;
        //Items = new ObservableCollection<BuyDocumentDto>(_navParameterService.BuyDocuments);
    }

    [RelayCommand]
    private async Task Appearing()
    {
        IsBusy = true;
        Items = await Task.Run(() =>
        {
            List<BusinessBuyDocUpdateItem> ritems = _navParameterService.BuyDocuments.Select(doc =>
                new BusinessBuyDocUpdateItem()
                {
                    Id = doc.Id,
                    BuyDocDefId = doc.BuyDocDefId,
                    BuyDocDefName = doc.BuyDocDefName,
                    NetAmount = doc.NetAmount,
                    VatAmount = doc.VatAmount,
                    PayedAmount = doc.PayedAmount,
                    RefNumber = doc.RefNumber,
                    SupplierId = doc.SupplierId,
                    SupplierName = doc.SupplierName,
                    TransDate = doc.TransDate,
                    BuyDocLines = doc.BuyDocLines?.Select(line => new BuyDocLineDto

                    {
                        Id = line.Id,
                        ItemId = line.ItemId,
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        UnitPrice = line.UnitPrice,
                        UnitQty = line.UnitQty,
                        LineDiscountAmount = line.LineDiscountAmount,
                        UnitDiscountRate = line.UnitDiscountRate,
                        LineNetAmount = line.LineNetAmount,
                        LineTotalAmount = line.LineTotalAmount,
                    }).ToList() ?? new List<BuyDocLineDto>(),
                }
            ).ToList();
            return new ObservableCollection<BusinessBuyDocUpdateItem>(ritems);
        });

        IsBusy = false;
        // return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SendToErp(BusinessBuyDocUpdateItem document) // Changed return type to Task and added async
    {

        if (document == null)
        {
            Debug.WriteLine("SendToErp cannot execute: document parameter is null.");
            return; // Return if document is null
        }
        Debug.WriteLine(
            $"SendToErp confirmed for document: Supplier={document.SupplierName}, Ref={document.RefNumber}");
        
        var updatedDocument = new BusinessBuyDocUpdateItem
        {
            Id = document.Id,
            BuyDocDefId = document.BuyDocDefId,
            BuyDocDefName = document.BuyDocDefName,
            NetAmount = document.NetAmount,
            VatAmount = document.VatAmount,
            PayedAmount = document.PayedAmount,
            RefNumber = document.RefNumber,
            SupplierId = document.SupplierId,
            SupplierName = document.SupplierName,
            TransDate = document.TransDate,
            BuyDocLines = document.BuyDocLines,
            Message = "Sending to ERP"
        };

        // Find the index of the old item and replace it
        int index = Items.IndexOf(document);
        if (index >= 0)
        {
            Items[index] = updatedDocument;
        }
       
    }
}