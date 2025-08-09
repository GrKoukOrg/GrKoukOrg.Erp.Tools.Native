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
    private readonly ILogger<DayCloseDataPageModel> _logger;
    private readonly ApiService _apiService;
    private readonly ISettingsDataService _settingsDataService;
    private readonly INavigationParameterService _navParameterService;
    private DateTime? _oldDateValue;
    
    [ObservableProperty] private bool _isWaitingForResponse = false;
    [ObservableProperty] private DateTime _closingDate = DateTime.Today;
    [ObservableProperty] private decimal _totalCash = 0;
    [ObservableProperty] private decimal _totalCards = 0;   
    [ObservableProperty] private decimal _totalStar = 0;
    [ObservableProperty] private decimal _totalSum = 0;
    [ObservableProperty] private int _zNumber = 0;
    [ObservableProperty] private bool _isOpen = false;
    private string _companyCode;
    private int _lastZNumber = 0;
    
    public DayCloseDataPageModel(ILogger<DayCloseDataPageModel> logger, ApiService apiService,
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
        try
        {
            _companyCode = _settingsDataService.GetBusinessCompanyCode();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        try
        {
            _lastZNumber = _settingsDataService.GetLastZNumber();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       
    }

    [RelayCommand]
    private async Task ShowPicker()
    {
        IsOpen=true;
    }

    [RelayCommand]
    private async Task PickerValueChanged(DatePickerSelectionChangedEventArgs e)
    {
        _oldDateValue = e.OldValue;
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
        ClosingDate=_oldDateValue ?? DateTime.Today;
        IsOpen=false;
    }

    [RelayCommand]

    partial void OnTotalCardsChanged(decimal value)
    {
        TotalSum=_totalCash+_totalCards+_totalStar;
    }

    partial void OnTotalCashChanged(decimal value)
    {
        TotalSum=_totalCash+_totalCards+_totalStar;
    }

    partial void OnTotalStarChanged(decimal value)
    {
        TotalSum=_totalCash+_totalCards+_totalStar;
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
            _settingsDataService.SetLastZNumber(_lastZNumber + 1);
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