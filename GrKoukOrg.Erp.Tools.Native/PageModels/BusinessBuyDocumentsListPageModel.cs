using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using GrKoukOrg.Erp.Tools.Native.Shared;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class BusinessBuyDocumentsListPageModel : ObservableObject
{
    private readonly ILogger<BusinessBuyDocumentsListPageModel> _logger;
    private readonly INavigationParameterService _navParameterService;
    private readonly ISettingsDataService _settingsDataService;
    private readonly ApiService _apiService;
    private readonly ModalErrorHandler _errorHandler;
    [ObservableProperty] private ObservableCollection<BusinessBuyDocUpdateItem> _items;
    [ObservableProperty] private bool _isBusy = false;
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(CancelStatusCheckCommand))]
    private bool _isCheckingStatus = false;
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(CheckStatusOfDocumentsCommand))]
    private bool _canCheckDocuments = false;
    [ObservableProperty] private int _checkedItems = 0;
    [ObservableProperty] private int _totalItems = 0;
    private CancellationTokenSource _cancellationTokenSource;

    public BusinessBuyDocumentsListPageModel(ILogger<BusinessBuyDocumentsListPageModel> logger
        , INavigationParameterService navParameterService,
        ISettingsDataService settingsDataService,
        ApiService apiService
        , ModalErrorHandler errorHandler)
    {
        _logger = logger;
        _navParameterService = navParameterService;
        _settingsDataService = settingsDataService;
        _apiService = apiService;
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
                    TotalAmount = doc.TotalAmount,
                    RefNumber = doc.RefNumber,
                    SupplierId = doc.SupplierId,
                    SupplierName = doc.SupplierName,
                    TransDate = doc.TransDate,
                    BuyDocLines = doc.BuyDocLines?.Select(line => new BuyDocLineDto
                    {
                        Id = line.Id,
                        BuyDocId = line.BuyDocId,
                        TransDate = line.TransDate,
                        ItemId = line.ItemId,
                        ItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        UnitPrice = line.UnitPrice,
                        UnitQty = line.UnitQty,
                        UnitFpaPerc = line.UnitFpaPerc,
                        UnitOfMeasureName = line.UnitOfMeasureName,
                        LineDiscountAmount = line.LineDiscountAmount,
                        UnitDiscountRate = line.UnitDiscountRate,
                        LineNetAmount = line.LineNetAmount,
                        LineVatAmount = line.LineVatAmount,
                        LineTotalAmount = line.LineTotalAmount,
                    }).ToList() ?? new List<BuyDocLineDto>(),
                }
            ).ToList();
            
            return new ObservableCollection<BusinessBuyDocUpdateItem>(ritems);
        });
        TotalItems = Items.Count;
        IsBusy = false;
        
        // await StartStatusCheck();
    }

    partial void OnTotalItemsChanged(int value)
    {
        CanCheckDocuments = value > 0;
    }
    [RelayCommand(CanExecute = nameof(CanCheckDocuments))]
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
    
    private async Task CheckDocumentStatus(BusinessBuyDocumentDto item)
    {
        try
        {
            //Send request to Erp
            var targetItem = Items.FirstOrDefault(x => x.Id == item.Id);
            var companyCode = _settingsDataService.GetBusinessCompanyCode();
            var erpApiBase = _settingsDataService.GetErpApiUrl(); 
            var erpApiUri = new Uri(erpApiBase + "/erpapi/SyncCheckBusinessBuyDocument");
            try
            {
                var payload = new SyncBusinessBuyDocumentRequest()
                {
                    Id = item.Id,
                    CompanyCode = companyCode,
                    BuyDocDefId = item.BuyDocDefId,
                    BuyDocDefName = item.BuyDocDefName,
                    VatAmount = item.VatAmount,
                    NetAmount = item.NetAmount,
                    TransDate = item.TransDate,
                    SupplierId = item.SupplierId,
                    RefNumber = item.RefNumber,
                    PayedAmount = item.PayedAmount,
                    TotalAmount = item.TotalAmount
                };
                var request = new HttpRequestMessage(HttpMethod.Post, erpApiUri)
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };
                var result = await _apiService.MakeAuthenticatedRequestAsync(request);
                var jsonContent = await result.Content.ReadAsStringAsync();
                var erpResponse = JsonSerializer.Deserialize<ErpCheckDocumentResponse>(jsonContent);

                var stMessage = erpResponse.Message;
                if (targetItem != null)
                {
                    UpdateMessageToItem(targetItem, stMessage);
                }

            }
            catch (Exception ex)
            {
                LogAndHandleException(ex, "An error occured while sending the suppliers sync request to Erp",targetItem);
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
    private async Task SendToErp(BusinessBuyDocUpdateItem document) 
    {
        Debug.WriteLine(
            $"SendToErp confirmed for document: Supplier={document.SupplierName}, Ref={document.RefNumber}");
        //Create the payload to send to Erp

        UpdateMessageToItem(document,"Sending to Erp");
        
        var targetItem = Items.FirstOrDefault(x => x.Id == document.Id);
        var companyCode = _settingsDataService.GetBusinessCompanyCode();
        var erpApiBase = _settingsDataService.GetErpApiUrl(); 
        var erpApiUri = new Uri(erpApiBase + "/erpapi/SyncAddBusinessBuyDocument");
        try
        {
            var payload = new SyncBusinessBuyDocumentRequest()
            {
                Id = document.Id,
                CompanyCode = companyCode,
                BuyDocDefId = document.BuyDocDefId,
                BuyDocDefName = document.BuyDocDefName,
                TransDate = document.TransDate,
                SupplierId = document.SupplierId,
                RefNumber = document.RefNumber,
                VatAmount = decimal.Abs(document.VatAmount),
                NetAmount = decimal.Abs( document.NetAmount),
                PayedAmount = document.PayedAmount,
                TotalAmount = decimal.Abs( document.TotalAmount)
            };
            var request = new HttpRequestMessage(HttpMethod.Post, erpApiUri)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            var result = await _apiService.MakeAuthenticatedRequestAsync(request);
            if (!result.IsSuccessStatusCode)
            {
                var errorContent = await result.Content.ReadAsStringAsync();
                if (targetItem != null)
                {
                    UpdateMessageToItem(targetItem, $"Error: {result.StatusCode} - {errorContent}");
                }
                return;
            }
            var jsonContent = await result.Content.ReadAsStringAsync();
            var erpResponse = JsonSerializer.Deserialize<ErpSynchronizationResponse<BuyDocumentDto>>(jsonContent);

            var stMessage = erpResponse.Message;
            if (targetItem != null)
            {
                UpdateMessageToItem(targetItem, stMessage);
            }

        }
        catch (Exception ex)
        {
            LogAndHandleException(ex, "An error occured while sending the suppliers sync request to Erp",targetItem);
        }
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
            TotalAmount = item.TotalAmount,
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
    private void LogAndHandleException(Exception ex, string customMessage,BusinessBuyDocUpdateItem targetItem)
    {
        string errorMessage = ex switch
        {
            TaskCanceledException when ex.InnerException is TimeoutException =>
                $"The request timed out: {ex.Message}",
            HttpRequestException =>
                "Network error: Unable to connect to the server. Please check your connection and try again.",
            JsonException =>
                "Data parsing error: The server returned data in an unexpected format.",
            TaskCanceledException =>
                "Request timeout: The server is taking too long to respond. Please try again later.",
            _ =>
                $"An unexpected error occurred: {ex.Message}"
        };
        if (targetItem is not null)
        {
            UpdateMessageToItem(targetItem, errorMessage);
        }
        Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");

        // Optionally, display the message using a Toast
        AppShell.DisplayToastAsync(customMessage ?? errorMessage).FireAndForgetSafeAsync();
    }
}