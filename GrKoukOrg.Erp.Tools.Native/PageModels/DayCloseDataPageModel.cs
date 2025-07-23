using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using GrKoukOrg.Erp.Tools.Native.Shared;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Picker;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class DayCloseDataPageModel:ObservableObject
{
    private readonly ILogger<BusBuyDocListSyncPageModel> _logger;
    private readonly ApiService _apiService;
    private readonly ISettingsDataService _settingsDataService;
    private readonly INavigationParameterService _navParameterService;
    
    
    [ObservableProperty] private bool _isWaitingForResponse = false;
    [ObservableProperty] private DateTime _closingDate = DateTime.Today;
    [ObservableProperty] private decimal _totalCash = 0;
    [ObservableProperty] private decimal _totalCards = 0;   
    [ObservableProperty] private decimal _totalStar = 0;
    [ObservableProperty] private int _zNumber = 0;
    [ObservableProperty] private bool _isOpen = false;
    private string _companyCode;
    private int _lastZNumber = 0;
    
    public DayCloseDataPageModel(ILogger<BusBuyDocListSyncPageModel> logger, ApiService apiService,
        ISettingsDataService settingsDataService, INavigationParameterService navParameterService)
    {
        _logger = logger;
        _apiService = apiService;
        _settingsDataService = settingsDataService;
        _navParameterService = navParameterService;
    }
    [RelayCommand]
    private async Task Appearing()
    {
        IsWaitingForResponse = false;
        _companyCode = _settingsDataService.GetBusinessCompanyCode();
        _lastZNumber=_settingsDataService.GetLastZNumber();
    }

    [RelayCommand]
    private async Task ShowPicker()
    {
        IsOpen=true;
    }

    [RelayCommand]
    private async Task PickerValueChanged(DatePickerSelectionChangedEventArgs e)
    {
        var oldvalue = e.OldValue;
        ClosingDate = (DateTime)e.NewValue;
    }
    [RelayCommand]
    private async Task AcceptPicker()
    {
        
        IsOpen=false;
    }
    [RelayCommand]
    private async Task DeclinePicker()
    {
        IsOpen=false;
    }

    [RelayCommand]
    private async Task Submit()
    {
        IsWaitingForResponse = true;
        var companyCode = _settingsDataService.GetBusinessCompanyCode();
        var dayClosePayload = new DayClosePayload()
        {
            CompanyCode = companyCode,
            TransDate = ClosingDate,
            ZNumber = ZNumber,
            TotalCash = TotalCash,
            TotalCards = TotalCards,
            TotalStar = TotalStar
        };
        var erpApiBase = _settingsDataService.GetErpApiUrl();
        var erpApiUri = new Uri(erpApiBase + "/erpapi/SyncAddBusinessDayCloseData");
        try
        {
           
            var request = new HttpRequestMessage(HttpMethod.Post, erpApiUri)
            {
                Content = new StringContent(JsonSerializer.Serialize(dayClosePayload), Encoding.UTF8, "application/json")
            };
            var result = await _apiService.MakeAuthenticatedRequestAsync(request);
            if (!result.IsSuccessStatusCode)
            {
                var errorContent = await result.Content.ReadAsStringAsync();
                await AppShell.DisplayToastAsync($"Error: {result.StatusCode} - {errorContent}");
                IsWaitingForResponse = false;
                return;
            }

            var jsonContent = await result.Content.ReadAsStringAsync();
            var erpResponse = JsonSerializer.Deserialize<DayCloseResponse>(jsonContent);

            var stMessage = erpResponse.Message;
            // var stMessage = 
            IsWaitingForResponse = false;
            await AppShell.DisplayToastAsync(stMessage);
        }
        catch (Exception ex)
        {
            IsWaitingForResponse = false;
            await AppShell.DisplayToastAsync($"Error: {ex.Message}");
            // LogAndHandleException(ex, "An error occured while sending the suppliers sync request to Erp");
        }

    }
}