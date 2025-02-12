using System.Text.Json;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.Services;

public interface IBusinessServerDataAccess
{
     Task<ICollection<ItemListDto>> GetBusinessServerItemsListAsync();
     
}

public class BusinessServerHttpDataAccess : IBusinessServerDataAccess
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BusinessServerHttpDataAccess> _logger;

    public BusinessServerHttpDataAccess(IHttpClientFactory httpClientFactory,ILogger<BusinessServerHttpDataAccess> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    public async Task<ICollection<ItemListDto>> GetBusinessServerItemsListAsync()
    {
        _logger.LogInformation("GetServerItemsListAsync");
        try
        {
            // Create HTTP client
            var client = _httpClientFactory.CreateClient("BusinessServerApi");
            client.Timeout = TimeSpan.FromSeconds(10);
            // Send GET request
            using var response = await client.GetAsync("/api/erpapi/getitems");
            response.EnsureSuccessStatusCode();

            // Read response content as JSON
            var jsonContent = await response.Content.ReadAsStringAsync();

            // Deserialize JSON into a list of ItemListDto
            var items = JsonSerializer.Deserialize<List<ItemListDto>>(jsonContent);

            // Return the items (or an empty list if deserialization fails)
            return items ?? new List<ItemListDto>();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            // Timeout specific handling
            _logger.LogError("The request timed out: {Message}", ex.Message);
            throw new TimeoutException("The server request timed out.", ex); // Optional rethrow with a specific exception
        }
        catch (HttpRequestException ex)
        {
            // Handle network-related errors
            _logger.LogError("A network-related error occurred: {Message}", ex.Message);
            throw; // Optional: Rethrow or return an empty list
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            _logger.LogError(ex, "Error in GetServerItemsListAsync");
            throw; // Rethrow the exception for the calling code to handle
        }

        
        
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Error in GetServerItemsListAsync");
        //    throw; // Rethrow the exception for the calling code to handle
        //   // return new List<ItemListDto>();
        // }

    }
}