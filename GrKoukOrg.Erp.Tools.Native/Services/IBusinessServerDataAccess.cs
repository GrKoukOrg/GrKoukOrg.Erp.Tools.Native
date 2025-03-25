using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GrKoukOrg.Erp.Tools.Native.Models;
using GrKoukOrg.Erp.Tools.Native.Shared;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.Services;

public interface IBusinessServerDataAccess
{
     Task<ICollection<ItemListDto>> GetBusinessServerItemsListAsync();
     Task<ICollection<SupplierListDto>> GetBusinessServerSupplierListAsync();
     Task<ICollection<BuyDocListDto>> GetBusinessServerBuyDocListAsync();
     Task<ICollection<BuyDocLineListDto>> GetBusinessServerBuyDocLineListAsync();
     Task<ICollection<CustomerListDto>> GetBusinessServerCustomerListAsync();
     Task<ICollection<SaleDocListDto>> GetBusinessServerSaleDocListAsync();
     Task<ICollection<SaleDocLineListDto>> GetBusinessServerSaleDocLineListAsync();
     Task<BusinessApiResponse<IList<ItemFamilyDto>>> GetBusinessServerItemFamilyListAsync();
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
    public async Task<ICollection<SupplierListDto>> GetBusinessServerSupplierListAsync()
    {
        _logger.LogInformation("GetServerSupplierListAsync");
        try
        {
            // Create HTTP client
            var client = _httpClientFactory.CreateClient("BusinessServerApi");
            client.Timeout = TimeSpan.FromSeconds(10);
            // Send GET request
            using var response = await client.GetAsync("/api/erpapi/getsuppliers");
            response.EnsureSuccessStatusCode();
            // Read response content as JSON
            var jsonContent = await response.Content.ReadAsStringAsync();
            // Deserialize JSON into a list of SupplierListDto
            var suppliers = JsonSerializer.Deserialize<List<SupplierListDto>>(jsonContent);
            // Return the suppliers (or an empty list if deserialization fails)
            return suppliers ?? new List<SupplierListDto>();
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
            _logger.LogError(ex, "Error in GetBusinessServerSupplierListAsync");
            throw; // Rethrow the exception for the calling code to handle
        }

    }
    public async Task<ICollection<CustomerListDto>> GetBusinessServerCustomerListAsync()
    {
        _logger.LogInformation("GetBusinessServerCustomerListAsync");
        try
        {
            // Create HTTP client
            var client = _httpClientFactory.CreateClient("BusinessServerApi");
            client.Timeout = TimeSpan.FromSeconds(10);
            // Send GET request
            using var response = await client.GetAsync("/api/erpapi/getCustomers");
            response.EnsureSuccessStatusCode();
            // Read response content as JSON
            var jsonContent = await response.Content.ReadAsStringAsync();
            // Deserialize JSON into a list of SupplierListDto
            var customers = JsonSerializer.Deserialize<List<CustomerListDto>>(jsonContent);
            // Return the suppliers (or an empty list if deserialization fails)
            return customers ?? new List<CustomerListDto>();
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
            _logger.LogError(ex, "Error in GetBusinessServerCustomerListAsync");
            throw; // Rethrow the exception for the calling code to handle
        }

    }
    public async Task<ICollection<BuyDocListDto>> GetBusinessServerBuyDocListAsync()
    {
        _logger.LogInformation("GetBusinessServerBuyDocListAsync");
        try
        {
            // Create HTTP client
            var client = _httpClientFactory.CreateClient("BusinessServerApi");
            client.Timeout = TimeSpan.FromSeconds(10);
            // Send GET request
            using var response = await client.GetAsync("/api/erpapi/GetBuyDocuments");
            response.EnsureSuccessStatusCode();
            // Read response content as JSON
            var jsonContent = await response.Content.ReadAsStringAsync();
            // Deserialize JSON into a list of SupplierListDto
            var buyDocList = JsonSerializer.Deserialize<List<BuyDocListDto>>(jsonContent);
            // Return the buydocListDto (or an empty list if deserialization fails)
            return buyDocList ?? new List<BuyDocListDto>();
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
            _logger.LogError(ex, "Error in GetBusinessServerBuyDocListAsync");
            throw; // Rethrow the exception for the calling code to handle
        }

    }
    public async Task<ICollection<SaleDocListDto>> GetBusinessServerSaleDocListAsync()
    {
        _logger.LogInformation("GetBusinessServerSaleDocListAsync");
        try
        {
            // Create HTTP client
            var client = _httpClientFactory.CreateClient("BusinessServerApi");
            client.Timeout = TimeSpan.FromSeconds(10);
            // Send GET request
            using var response = await client.GetAsync("/api/erpapi/GetSaleDocuments");
            response.EnsureSuccessStatusCode();
            // Read response content as JSON
            var jsonContent = await response.Content.ReadAsStringAsync();
            // Deserialize JSON into a list of SupplierListDto
            var saleDocList = JsonSerializer.Deserialize<List<SaleDocListDto>>(jsonContent);
            // Return the buydocListDto (or an empty list if deserialization fails)
            return saleDocList ?? new List<SaleDocListDto>();
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
            _logger.LogError(ex, "Error in GetBusinessServerSaleDocListAsync");
            throw; // Rethrow the exception for the calling code to handle
        }

    }
    public async Task<ICollection<BuyDocLineListDto>> GetBusinessServerBuyDocLineListAsync()
    {
        _logger.LogInformation("GetBusinessServerBuyDocLineListAsync");
        try
        {
            // Create HTTP client
            var client = _httpClientFactory.CreateClient("BusinessServerApi");
            client.Timeout = TimeSpan.FromSeconds(10);
            // Send GET request
            using var response = await client.GetAsync("/api/erpapi/GetBuyDocLines");
            response.EnsureSuccessStatusCode();
            // Read response content as JSON
            var jsonContent = await response.Content.ReadAsStringAsync();
            // Deserialize JSON into a list of SupplierListDto
            var buyDocLineList = JsonSerializer.Deserialize<List<BuyDocLineListDto>>(jsonContent);
            // Return the buyDocLineListDto (or an empty list if deserialization fails)
            return buyDocLineList ?? new List<BuyDocLineListDto>();
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
            _logger.LogError(ex, "Error in GetBusinessServerBuyDocListAsync");
            throw; // Rethrow the exception for the calling code to handle
        }

    }
    public async Task<ICollection<SaleDocLineListDto>> GetBusinessServerSaleDocLineListAsync()
    {
        _logger.LogInformation("GetBusinessServerSaleDocLineListAsync");
        try
        {
            // Create HTTP client
            var client = _httpClientFactory.CreateClient("BusinessServerApi");
            client.Timeout = TimeSpan.FromSeconds(10);
            // Send GET request
            using var response = await client.GetAsync("/api/erpapi/GetSaleDocLines");
            response.EnsureSuccessStatusCode();
            // Read response content as JSON
            var jsonContent = await response.Content.ReadAsStringAsync();
            // Deserialize JSON into a list of SupplierListDto
            var saleDocLineList = JsonSerializer.Deserialize<List<SaleDocLineListDto>>(jsonContent);
            // Return the buyDocLineListDto (or an empty list if deserialization fails)
            return saleDocLineList ?? new List<SaleDocLineListDto>();
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
            _logger.LogError(ex, "Error in GetBusinessServerSaleDocListAsync");
            throw; // Rethrow the exception for the calling code to handle
        }

    }

    public async Task<BusinessApiResponse<IList<ItemFamilyDto>>> GetBusinessServerItemFamilyListAsync()
    {
        _logger.LogInformation("GetBusinessServerItemFamilyListAsync");
        try
        {
            // Create HTTP client
            var client = _httpClientFactory.CreateClient("BusinessServerApi");
            client.Timeout = TimeSpan.FromSeconds(10);
            // Send GET request
            using var response = await client.GetAsync("/api/erpapi/GetItemFamilies");
            response.EnsureSuccessStatusCode();

            // Read response content as JSON
            var jsonContent = await response.Content.ReadAsStringAsync();

            // Deserialize JSON 
            
            var apiResponse = JsonSerializer.Deserialize<BusinessApiResponse<IList<ItemFamilyDto>>>(jsonContent);

          
            return apiResponse ?? new BusinessApiResponse<IList<ItemFamilyDto>>();
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

       
    }
}