using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using GrKoukOrg.Erp.Tools.Native.Shared;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class SyncToErpPageModel:ObservableObject
{
    private readonly ApiService _apiService;
    private readonly ISettingsDataService _settingsDataService;
    private readonly IBusinessServerDataAccess _businessServerDataAccess;

    [ObservableProperty] bool _isBusy;
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    
    public SyncToErpPageModel(ApiService apiService, ISettingsDataService settingsDataService, IBusinessServerDataAccess businessServerDataAccess)
    {
        _apiService = apiService;
        _settingsDataService = settingsDataService;
        _businessServerDataAccess = businessServerDataAccess;
    }
    
    [RelayCommand]
    private async Task SyncItemFamilies()
    {
        //Get ItemFamilies from Business Server
        try
        {
            var apiResponse=await _businessServerDataAccess.GetBusinessServerItemFamilyListAsync();
            var items = apiResponse.Items;
            // if (items == null)
            // {
            //     return;
            // }

            if (items.Count == 0)
            {
                return;
            }
            var erpApiBase = _settingsDataService.GetErpApiUrl(); 
            var erpApiUri = new Uri(erpApiBase + "/erpapi/SyncBusinessItemFamilies");
            string businessCompanyCode = apiResponse.CompanyCode;
            try
            {
                var payload = new SyncBusinessItemFamilyRequest()
                {
                    CompanyCode = businessCompanyCode,
                    Items = items
                };
                var request = new HttpRequestMessage(HttpMethod.Post, erpApiUri)
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };
                var result = await _apiService.MakeAuthenticatedRequestAsync(request);
                var jsonContent = await result.Content.ReadAsStringAsync();
                var erpResponse = JsonSerializer.Deserialize<ErpSynchronizationResponse<ItemFamilyDto>>(jsonContent);
                 
                var stMessage = $"Message: {erpResponse.Message}\n" +
                                $"Added Count: {erpResponse.AddedCount}\n" +
                                $"Failed to Add Count: {erpResponse.FailedToAddCount}\n" +
                                $"Updated Count: {erpResponse.UpdatedCount}\n" +
                                $"Failed to Update Count: {erpResponse.FailedToUpdateCount}\n" +
                                $"Deleted Count: {erpResponse.DeletedCount}\n" +
                                $"Failed to Delete Count: {erpResponse.FailedToDeleteCount}";
                StatusMessage = stMessage;
                //StatusMessage=result.ToString();
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Console.WriteLine(ex);
                await AppShell.DisplayToastAsync(StatusMessage);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            await AppShell.DisplayToastAsync(StatusMessage);
        }
      
        
    }
    [RelayCommand]
    private async Task SyncUnitsOfMeasurement()
    {
        //Get SyncUnitsOfMeasurement from Business Server
        try
        {
            var apiResponse=await _businessServerDataAccess.GetBusinessServerUnitsOfMeasurementListAsync();
            var items = apiResponse.Items;
            // if (items == null)
            // {
            //     return;
            // }

            if (items.Count == 0)
            {
                return;
            }
            var erpApiBase = _settingsDataService.GetErpApiUrl(); 
            var erpApiUri = new Uri(erpApiBase + "/erpapi/SyncBusinessUnitsOfMeasurement");
            string businessCompanyCode = apiResponse.CompanyCode;
            try
            {
                var payload = new SyncBusinessUnitsOfMeasurementRequest()
                {
                    CompanyCode = businessCompanyCode,
                    Items = items
                };
                var request = new HttpRequestMessage(HttpMethod.Post, erpApiUri)
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };
                var result = await _apiService.MakeAuthenticatedRequestAsync(request);
                StatusMessage=result.ToString();
                var jsonContent = await result.Content.ReadAsStringAsync();
                var erpResponse = JsonSerializer.Deserialize<ErpSynchronizationResponse<UnitOfMeasurementDto>>(jsonContent);
                 
                var stMessage = $"Message: {erpResponse.Message}\n" +
                                $"Added Count: {erpResponse.AddedCount}\n" +
                                $"Failed to Add Count: {erpResponse.FailedToAddCount}\n" +
                                $"Updated Count: {erpResponse.UpdatedCount}\n" +
                                $"Failed to Update Count: {erpResponse.FailedToUpdateCount}\n" +
                                $"Deleted Count: {erpResponse.DeletedCount}\n" +
                                $"Failed to Delete Count: {erpResponse.FailedToDeleteCount}";
                StatusMessage = stMessage;
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Console.WriteLine(ex);
                await AppShell.DisplayToastAsync(StatusMessage);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            await AppShell.DisplayToastAsync(StatusMessage);
        }
      
        
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

    [RelayCommand]
    private async Task SyncSuppliers()
    {
        //open sync suppliers page to perform sync
        await Shell.Current.GoToAsync("syncSuppliers");
    }
}