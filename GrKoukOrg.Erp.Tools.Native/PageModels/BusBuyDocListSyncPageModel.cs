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
    private readonly IBusinessServerDataAccess _businessServerDataAccess;

    [ObservableProperty] private bool _isProgressBarVisible = false;
    [ObservableProperty] private int _operationProgress = 0;
    [ObservableProperty] private bool _isWaitingForResponse = false;
    [ObservableProperty] bool _isBusy;
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    
    public ObservableCollection<LogEntry> LogEntries { get; } = new();
    
    [ObservableProperty] private int _lastLogEntryIndex;
    public BusBuyDocListSyncPageModel(ILogger<BusBuyDocListSyncPageModel> logger
        , ApiService apiService
        , ISettingsDataService settingsDataService
        , IBusinessServerDataAccess businessServerDataAccess)
    {
        _logger = logger;
        _apiService = apiService;
        _settingsDataService = settingsDataService;
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
    }
    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Please enter both username and password.";
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
            }
            else
            {
                StatusMessage = "Invalid login credentials.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}