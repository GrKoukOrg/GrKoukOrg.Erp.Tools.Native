using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Services;
using Microsoft.Maui.Storage;

public interface ISettingsDataService
{
    Task<string> GetBusinessApiUrlAsync();
    Task<int> SetBusinessApiUrlAsync(string url);
    string GetBusinessApiUrl();
    int SetBusinessApiUrl(string url);
    Task<string> GetErpApiUrlAsync();
    Task<int> SetErpApiUrlAsync(string url);
    string GetErpApiUrl();
    int SetErpApiUrl(string url);
}

public class SettingsMemoryDataService : ISettingsDataService
{
    private const string businessApiBaseDefaultUrl= "http://192.168.20.38:1234";
    private const string erpApiBaseDefaultUrl= "https://192.168.20.38:5001/";
    public async Task<string> GetBusinessApiUrlAsync()
    {
        string url = Preferences.Get("businessApiUrl",businessApiBaseDefaultUrl );
        
        return await Task.FromResult( url);
    }

    public string GetBusinessApiUrl()
    {
        string url = Preferences.Get("businessApiUrl", businessApiBaseDefaultUrl);
        return url;
    }
    public Task<int> SetBusinessApiUrlAsync(string url)
    {
        Preferences.Set("businessApiUrl",url);
        return Task.FromResult(0);
    }
    public int SetBusinessApiUrl(string url)
    {
        Preferences.Set("businessApiUrl",url);
        return 0;
    }
    
    public async Task<string> GetErpApiUrlAsync()
    {
        string url = Preferences.Get("erpApiUrl",erpApiBaseDefaultUrl );
        
        return await Task.FromResult( url);
    }

    public string GetErpApiUrl()
    {
        string url = Preferences.Get("erpApiUrl", erpApiBaseDefaultUrl);
        return url;
    }
    public Task<int> SetErpApiUrlAsync(string url)
    {
        Preferences.Set("erpApiUrl",url);
        return Task.FromResult(0);
    }
    public int SetErpApiUrl(string url)
    {
        Preferences.Set("erpApiUrl",url);
        return 0;
    }
}

public interface IBusinessDataService
{
    Task<ItemListDto> GetItems();
}

public class BusinessDbDataService : IBusinessDataService
{
    public BusinessDbDataService()
    {
        
    }
    public Task<ItemListDto> GetItems()
    {
        throw new NotImplementedException();
    }
    
}