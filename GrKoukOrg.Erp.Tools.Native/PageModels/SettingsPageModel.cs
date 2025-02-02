using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class SettingsPageModel:ObservableObject
{
    
    private readonly ISettingsDataService _settingsDataService;
    
    [ObservableProperty]
    private string _apiUrl = string.Empty;
   
    // ReSharper disable once ConvertToPrimaryConstructor
    public SettingsPageModel(ISettingsDataService settingsDataService)
    {
        _settingsDataService = settingsDataService;
    }
    
    [RelayCommand]
    private async Task Appearing()
    {
        ApiUrl = await _settingsDataService.GetApiUrlAsync();
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            await _settingsDataService.SetApiUrlAsync(ApiUrl);
            await AppShell.DisplayToastAsync("Settings saved");
        }
        catch (Exception e)
        {
            await AppShell.DisplayToastAsync($"Error: {e.Message}");
        }
    }
}       