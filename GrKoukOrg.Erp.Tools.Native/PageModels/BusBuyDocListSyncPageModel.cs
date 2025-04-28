using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class BusBuyDocListSyncPageModel : ObservableObject
{
    private readonly ILogger<BusBuyDocListSyncPageModel> _logger;
    private readonly ApiService _apiService;
    private readonly ISettingsDataService _settingsDataService;
    private readonly INavigationParameterService _navParameterService;
    private readonly IBusinessServerDataAccess _businessServerDataAccess;

    [ObservableProperty] private bool _isProgressBarVisible = false;
    [ObservableProperty] private int _operationProgress = 0;
    [ObservableProperty] private bool _isWaitingForResponse = false;
    [ObservableProperty] bool _isBusy;
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private int _buyDocCount = 0;
    [ObservableProperty] private int _saleDocCount = 0;
    [ObservableProperty] private DateTime _fromDate;
    [ObservableProperty] private DateTime _toDate;
    public ObservableCollection<LogEntry> LogEntries { get; } = new();
    [ObservableProperty] private int _lastLogEntryIndex;
    
    private IList<BusinessBuyDocumentDto> _buyDocuments;
    public BusBuyDocListSyncPageModel(ILogger<BusBuyDocListSyncPageModel> logger
        ,ApiService apiService
        ,ISettingsDataService settingsDataService
        ,INavigationParameterService navParameterService
        ,IBusinessServerDataAccess businessServerDataAccess
        )
    {
        _logger = logger;
        _apiService = apiService;
        _settingsDataService = settingsDataService;
        _navParameterService = navParameterService;
        _businessServerDataAccess = businessServerDataAccess;
    }
    
    private void AddLog(string message)
    {
        LogEntries.Insert(0, new LogEntry { Timestamp = DateTime.Now.ToString("HH:mm:ss"), Message = message });
    }
    
    [RelayCommand]
    private async Task Appearing()
    {
        IsProgressBarVisible = false;
        IsWaitingForResponse = false;
        ToDate=DateTime.Today;
        FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

    }
    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Please enter both username and password.";
            await AppShell.DisplayToastAsync(StatusMessage);
            return;
        }

        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            var tokens = await _apiService.LoginAsync(UserName, Password);
            if (tokens != null)
            {
                Preferences.Default.Set("AccessToken", tokens.AccessToken);
                Preferences.Default.Set("RefreshToken", tokens.RefreshToken);

                StatusMessage = "Login successful!";
                await AppShell.DisplayToastAsync(StatusMessage);
            }
            else
            {
                StatusMessage = "Invalid login credentials.";
                await AppShell.DisplayToastAsync(StatusMessage);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            await AppShell.DisplayToastAsync(StatusMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task GetSaleDocs()
    {
        await GetSaleDocumentsProcessAsync();
    }

    private async Task GetSaleDocumentsProcessAsync()
    {
        throw new NotImplementedException();
    }
    
    [RelayCommand]
    private async Task GetBuyDocs()
    {
        await GetBuyDocumentsProcessAsync();
    }
   
    [RelayCommand]
    private async Task ShowBuyDocuments()
    {
        if (_buyDocuments is null || _buyDocuments.Count == 0)
        {
            return;
        }
        _navParameterService.BuyDocuments = _buyDocuments;
      

        await Shell.Current.GoToAsync("Businessbuydoclist");
    
    }

    private async Task ShowBuyDocumentsProcessAsync()
    {
        AddLog("Getting Buy Doc from business server");
    }

    private async Task GetBuyDocumentsProcessAsync()
    {
        AddLog("Getting Buy Doc from business server");
        try
        {
            IsWaitingForResponse = true;
            
            var response = await _businessServerDataAccess.GetBusinessServerBuyDocsInPeriodListAsync(DateOnly.FromDateTime(FromDate), DateOnly.FromDateTime(ToDate));
            _buyDocuments = response.Items;
            BuyDocCount = _buyDocuments.Count;
            AddLog($"Connected and retrieved {BuyDocCount} Buy Documents");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetBuyDocumentsProcessAsync");
            AddLog($"Error in GetBuyDocumentsProcessAsync: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetBuyDocumentsProcessAsync: {e.Message}");
        }
        finally
        {
            IsWaitingForResponse = false;
        }
    }
}