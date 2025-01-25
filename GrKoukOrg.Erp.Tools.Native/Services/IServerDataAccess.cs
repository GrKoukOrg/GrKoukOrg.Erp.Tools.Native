using System.Text.Json;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.Services;

public interface IServerDataAccess
{
     Task<ICollection<ItemListDto>> GetServerItemsListAsync();
     
}

public class ServerHttpDataAccess : IServerDataAccess
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ServerHttpDataAccess> _logger;

    public ServerHttpDataAccess(IHttpClientFactory httpClientFactory,ILogger<ServerHttpDataAccess> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    public async Task<ICollection<ItemListDto>> GetServerItemsListAsync()
    {
        _logger.LogInformation("GetServerItemsListAsync");
        try
        {
            // Create HTTP client
            var client = _httpClientFactory.CreateClient("ServerHttpDataAccess");

            // Send GET request
            using var response = await client.GetAsync("api/erpapi/getitems");
            response.EnsureSuccessStatusCode();

            // Read response content as JSON
            var jsonContent = await response.Content.ReadAsStringAsync();

            // Deserialize JSON into a list of ItemListDto
            var items = JsonSerializer.Deserialize<List<ItemListDto>>(jsonContent);

            // Return the items (or an empty list if deserialization fails)
            return items ?? new List<ItemListDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetServerItemsListAsync");
            throw; // Rethrow the exception for the calling code to handle
        }

    }
}