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
    private async Task Action(BuyDocumentDto document) // Changed return type to Task and added async
    {
        Console.WriteLine($"Attempting to execute action..."); // More specific logging
        if (document == null)
        {
            Console.WriteLine("Action cannot execute: document parameter is null.");
            // Optionally display an error to the user
            if (Application.Current?.MainPage != null) {
                await Application.Current.MainPage.DisplayAlert("Error", "Document data is missing.", "OK");
            }
            return; // Return if document is null
        }

        // Add your actual logic here. Example:
        Console.WriteLine($"Action confirmed for document: Supplier={document.SupplierName}, Ref={document.RefNumber}");
        if (Application.Current?.MainPage != null) {
            await Application.Current.MainPage.DisplayAlert("Action Triggered", $"You clicked on {document.SupplierName}", "OK");
        }
        // Add other logic like navigation, data processing, etc.
    }


}