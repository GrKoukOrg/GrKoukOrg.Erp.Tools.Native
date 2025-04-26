using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            await _settingsDataService.SetBusinessApiUrlAsync(BusinessApiUrl);
            await _settingsDataService.SetErpApiUrlAsync(ErpApiUrl);
             _settingsDataService.SetBusinessCompanyCode(CompanyCode);
            await AppShell.DisplayToastAsync("Settings saved");
           // await AppShell.DisplaySnackbarAsync("Settings saved");
        }
        catch (Exception e)
        {
            await AppShell.DisplayToastAsync($"Error: {e.Message}");
        }
    }
}       