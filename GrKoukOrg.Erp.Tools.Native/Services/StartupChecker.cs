using GrKoukOrg.Erp.Tools.Native.Models;
namespace GrKoukOrg.Erp.Tools.Native.Services;

public class StartupChecker : IStartupChecker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingsDataService _settingsDataService;
    private readonly ApiService _apiService;
    public StartupChecker(
        IHttpClientFactory httpClientFactory,
        ISettingsDataService settingsDataService,
        ApiService apiService)
    {
        _httpClientFactory = httpClientFactory;
        _settingsDataService = settingsDataService;
        _apiService = apiService;
    }

    public async Task<StartupCheckResult> PerformAllChecksAsync()
    {
        var result = new StartupCheckResult();
        try
        {
            // Check local network only (does not guarantee internet)
            result.NetworkConnected = Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

            // Check Internet Access with external host
            var httpClient = _httpClientFactory.CreateClient();
            try
            {
                // using var internetResp = await httpClient.GetAsync("https://www.msftconnecttest.com/connecttest.txt");
                using var internetResp = await httpClient.GetAsync("https://8.8.8.8");
                result.InternetAccessible = internetResp.IsSuccessStatusCode;
            }
            catch
            {
                result.InternetAccessible = false;
            }

            // API Connectivity using ISettingsDataService
            string apiUrl = _settingsDataService.GetErpApiUrl().TrimEnd('/');
            try
            {
                var apiClient = _httpClientFactory.CreateClient("ErpApi");
                apiClient.BaseAddress = new Uri(apiUrl);
                using var apiResp = await apiClient.GetAsync("/health"); // You can use a ping/health endpoint
                result.ApiConnected = apiResp.IsSuccessStatusCode;
            }
            catch
            {
                result.ApiConnected = false;
            }

            // API Token Refresh
            try
            {
                var token = await _apiService.RefreshTokenAsync();
                result.TokenRefreshSuccess = !string.IsNullOrEmpty(token?.AccessToken);
                Preferences.Set("AccessToken", token.AccessToken);
                Preferences.Set("RefreshToken", token.RefreshToken);
            }
            catch (Exception ex)
            {
                result.TokenRefreshSuccess = false;
                result.ErrorMessage = ex.Message;
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"General startup check error: {ex.Message}";
        }
        return result;
    }
}