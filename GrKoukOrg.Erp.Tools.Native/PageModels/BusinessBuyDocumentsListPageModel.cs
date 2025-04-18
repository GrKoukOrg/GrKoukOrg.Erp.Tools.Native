using System.Collections.ObjectModel;
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
    [ObservableProperty] private ObservableCollection<BuyDocumentDto> _items;
    [ObservableProperty] private bool _isBusy = false;
    public BusinessBuyDocumentsListPageModel(ILogger<BusinessBuyDocumentsListPageModel> logger
        ,INavigationParameterService navParameterService
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
        Items = await Task.Run(() => new ObservableCollection<BuyDocumentDto>(_navParameterService.BuyDocuments));
        IsBusy = false;
       // return Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task Action(BuyDocumentDto document)
    {
        if (document == null)
            return;

        // Add your logic here. Example:
       // await AppShell.DisplayAlertAsync("Action Triggered", $"You clicked on {document.SupplierName}", "OK");
    }

}