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
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(CancelStatusCheckCommand))]
    private bool _isCheckingStatus = false;
    [ObservableProperty] private int _checkedItems = 0;
    [ObservableProperty] private int _totalItems = 0;
    private CancellationTokenSource _cancellationTokenSource;

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
        
        // await StartStatusCheck();
    }

    [RelayCommand]
    private async Task CheckStatusOfDocuments()
    {
        await StartStatusCheck();
    }

   
    private async Task StartStatusCheck()
    {
        IsCheckingStatus = true;
        CheckedItems = 0;
        TotalItems = Items.Count;
        _cancellationTokenSource = new CancellationTokenSource();
    
        try
        {
            foreach (var item in _navParameterService.BuyDocuments)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;
    
                await CheckDocumentStatus(item);
                CheckedItems++;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Status check cancelled");
        }
        finally
        {
            IsCheckingStatus = false;
            _cancellationTokenSource = null;
        }
    }
    
    private async Task CheckDocumentStatus(BuyDocumentDto item)
    {
        try
        {
            await Task.Delay(1000, _cancellationTokenSource.Token);
            var targetItem = Items.FirstOrDefault(x => x.Id == item.Id);
            if (targetItem != null)
            {
                UpdateMessageToItem(targetItem, "Checked with ERP");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking document status");
            var targetItem = Items.FirstOrDefault(x => x.Id == item.Id);
            if (targetItem != null)
            {
                UpdateMessageToItem(targetItem, "Error checking status");
            }
        }
    }
    
    [RelayCommand(CanExecute = nameof(IsCheckingStatus))]
    private void CancelStatusCheck()
    {
        _cancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    private async Task SendToErp(BusinessBuyDocUpdateItem document) // Changed return type to Task and added async
    {
        Debug.WriteLine(
            $"SendToErp confirmed for document: Supplier={document.SupplierName}, Ref={document.RefNumber}");
        //Create the payload to send to Erp

        UpdateMessageToItem(document,"Sending to Erp");
    }

    private void UpdateMessageToItem(BusinessBuyDocUpdateItem item, string message )
    {
        var updatedDocument = new BusinessBuyDocUpdateItem
        {
            Id = item.Id,
            BuyDocDefId = item.BuyDocDefId,
            BuyDocDefName = item.BuyDocDefName,
            NetAmount = item.NetAmount,
            VatAmount = item.VatAmount,
            PayedAmount = item.PayedAmount,
            RefNumber = item.RefNumber,
            SupplierId = item.SupplierId,
            SupplierName = item.SupplierName,
            TransDate = item.TransDate,
            BuyDocLines = item.BuyDocLines,
            Message = message
        };

        // Find the index of the old item and replace it
        int index = Items.IndexOf(item);
        if (index >= 0)
        {
            Items[index] = updatedDocument;
        }
    }
}