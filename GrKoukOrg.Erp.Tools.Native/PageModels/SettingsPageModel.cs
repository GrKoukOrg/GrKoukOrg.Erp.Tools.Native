using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Data;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class SettingsPageModel:ObservableObject
{
    
    private readonly ISettingsDataService _settingsDataService;
    
    [ObservableProperty]
    private string _businessApiUrl = string.Empty;
    [ObservableProperty]
    private string _erpApiUrl = string.Empty;
    
    [ObservableProperty]
    private string _companyCode = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _databasePath = string.Empty;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SettingsPageModel(ISettingsDataService settingsDataService)
    {
        _settingsDataService = settingsDataService;
    }
    
    [RelayCommand]
    private async Task Appearing()
    {
        BusinessApiUrl = await _settingsDataService.GetBusinessApiUrlAsync();
        ErpApiUrl = await _settingsDataService.GetErpApiUrlAsync();
        CompanyCode=  _settingsDataService.GetBusinessCompanyCode();
        // Show the actual file path used by SQLite (without the connection string prefix)
        try
        {
            // Constants.DatabasePath is connection string like "Data Source=..."; extract the actual path for display
            var connStr = Constants.DatabasePath;
            const string prefix = "Data Source=";
            DatabasePath = connStr.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? connStr.Substring(prefix.Length)
                : connStr;
        }
        catch
        {
            DatabasePath = string.Empty;
        }
        UpdateStatusMessage("Loaded current settings");
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            // Basic validation to inform the user better
            if (string.IsNullOrWhiteSpace(CompanyCode) || string.IsNullOrWhiteSpace(BusinessApiUrl) || string.IsNullOrWhiteSpace(ErpApiUrl))
            {
                await AppShell.DisplayToastAsync("Please fill in Company Code, Business Api Url and Erp Api Url before saving.");
                return;
            }

            await _settingsDataService.SetBusinessApiUrlAsync(BusinessApiUrl);
            await _settingsDataService.SetErpApiUrlAsync(ErpApiUrl);
            _settingsDataService.SetBusinessCompanyCode(CompanyCode);

            UpdateStatusMessage("Settings saved");

            await AppShell.DisplayToastAsync($"Settings saved on {DateTime.Now:G}\n{BuildDetails()}\nPlatform: {RuntimeInformation.OSDescription}");
            
            // await AppShell.DisplaySnackbarAsync("Settings saved");
        }
        catch (Exception e)
        {
            await AppShell.DisplayToastAsync($"Error: {e.Message}");
        }
    }

    [RelayCommand]
    private async Task Reset()
    {
        
    }
    private void UpdateStatusMessage(string prefix)
    {
        StatusMessage = $"{prefix}:\n{BuildDetails()}\nLast update: {DateTime.Now:G}";
    }

    private string BuildDetails()
    {
        return $"Company: {CompanyCode}\nBusiness API: {BusinessApiUrl}\nERP API: {ErpApiUrl}";
    }
}       