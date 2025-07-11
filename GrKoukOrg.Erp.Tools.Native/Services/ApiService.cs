using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Maui.Storage;

namespace GrKoukOrg.Erp.Tools.Native.Services;

public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingsDataService _settingsDataService;

    private readonly HttpClient _httpClient;

    public ApiService(IHttpClientFactory httpClientFactory,ISettingsDataService settingsDataService)
    {
        _httpClientFactory = httpClientFactory;
        _settingsDataService = settingsDataService;
        var apiBaseUrl = _settingsDataService.GetErpApiUrl(); 
        _httpClient = new HttpClient
        {
          //BaseAddress = new Uri(apiBaseUrl),
          //Timeout = TimeSpan.FromSeconds(10)
              
        };
    }

    // Login API call
    public async Task<TokenModel> LoginAsync(string username, string password)
    {
        var apiBaseUrl = _settingsDataService.GetErpApiUrl(); 
        var url = new Uri(apiBaseUrl + "/auth/login");
        
        var loginData = new
        {
            Username = username,
            Password = password
        };

        try
        {
            var response = await _httpClient.PostAsync(url,
                new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TokenModel>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            // Timeout specific handling
            throw new TimeoutException("The server request timed out.", ex); // Optional rethrow with a specific exception
        }
        catch (HttpRequestException ex)
        {
            // Handle network-related errors
            Console.WriteLine(ex);
            throw; // Optional: Rethrow or return an empty list
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return null;
    }

    // Refresh token logic
    public async Task<TokenModel> RefreshTokenAsync()
    {
        var accessToken =  Preferences.Default.Get("AccessToken",string.Empty);
        var refreshToken =  Preferences.Default.Get("RefreshToken",string.Empty);

        var refreshData = new
        {
            Token = accessToken,
            RefreshToken = refreshToken
        };
        var apiBaseUrl = _settingsDataService.GetErpApiUrl(); 
        var url = new Uri(apiBaseUrl + "/auth/refresh");

        try
        {
            var response = await _httpClient.PostAsync(url,
                new StringContent(JsonSerializer.Serialize(refreshData), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                try
                {
                    var retTokenModel = JsonSerializer.Deserialize<TokenModel>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (retTokenModel == null)
                    {
                        return new TokenModel()
                        {
                            AccessToken = String.Empty,
                            RefreshToken = string.Empty
                        };
                    }
                    return retTokenModel;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new TokenModel()
                    {
                        AccessToken = String.Empty,
                        RefreshToken = string.Empty
                    };
                    
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        throw new Exception("Token refresh failed.");
    }
    
    // Decode roles from the access token
    public async Task<List<string>> GetRolesFromAccessTokenAsync()
    {
        // Retrieve the AccessToken from SecureStorage
        var accessToken = await SecureStorage.GetAsync("AccessToken");
        if (accessToken == null)
        {
            throw new Exception("Access token not found.");
        }

        // Decode the AccessToken
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);

        // Extract Roles from Claims
        var roles = jwtToken.Claims
            .Where(claim => claim.Type == ClaimTypes.Role) // Role claims often use ClaimTypes.Role
            .Select(claim => claim.Value)
            .ToList();

        return roles;
    }

    // Use this method where needed to enforce role-based behavior
    public async Task<bool> IsUserInRoleAsync(string roleName)
    {
        var roles = await GetRolesFromAccessTokenAsync();
        return roles.Contains(roleName);
    }
    private  async Task AddAuthorizationHeaderAsync()
    {
        
        var accessToken =  Preferences.Default.Get("AccessToken",string.Empty);

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
    public async Task<HttpResponseMessage> MakeAuthenticatedRequestAsync(HttpRequestMessage request)
    {
        await AddAuthorizationHeaderAsync();
    
        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Try to refresh token
            try
            {
                var newTokenModel = await RefreshTokenAsync();

                // Save the new tokens
                Preferences.Set("AccessToken", newTokenModel.AccessToken);
                Preferences.Set("RefreshToken", newTokenModel.RefreshToken);

                // Retry the request with the new access token
                var newRequest = CloneHttpRequestMessage(request); 

                await AddAuthorizationHeaderAsync();
                response = await _httpClient.SendAsync(newRequest);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to refresh token.", ex);
            }
        }

        return response;
    }
// Helper method to clone an HttpRequestMessage
    private HttpRequestMessage CloneHttpRequestMessage(HttpRequestMessage request)
    {
        var newRequest = new HttpRequestMessage(request.Method, request.RequestUri);

        // Copy headers
        foreach (var header in request.Headers)
        {
            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content, if any
        if (request.Content != null)
        {
            newRequest.Content = new StreamContent(request.Content.ReadAsStream());
            foreach (var header in request.Content.Headers)
            {
                newRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return newRequest;
    }

}