using System.Text;
using System.Text.Json;
using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Services;

public class ApiService
{
    private const string ApiBaseUrl = "https://localhost:5001/api/erpapi/";

    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiBaseUrl)
        };
    }

    // Login API call
    public async Task<TokenModel> LoginAsync(string username, string password)
    {
        var loginData = new
        {
            Username = username,
            Password = password
        };

        var response = await _httpClient.PostAsync("/auth/login",
            new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenModel>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return null;
    }

    // Refresh token logic
    public async Task<TokenModel> RefreshTokenAsync()
    {
        var accessToken = await SecureStorage.GetAsync("AccessToken");
        var refreshToken = await SecureStorage.GetAsync("RefreshToken");

        var refreshData = new
        {
            Token = accessToken,
            RefreshToken = refreshToken
        };

        var response = await _httpClient.PostAsync("/auth/refresh",
            new StringContent(JsonSerializer.Serialize(refreshData), Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenModel>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        throw new Exception("Token refresh failed.");
    }

}