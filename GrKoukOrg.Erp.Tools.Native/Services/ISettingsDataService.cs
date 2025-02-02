using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Services;
using Microsoft.Maui.Storage;

public interface ISettingsDataService
{
    Task<string> GetApiUrlAsync();
    Task<int> SetApiUrlAsync(string url);
    string GetApiUrl();
    int SetApiUrl(string url);
}

public class SettingsMemoryDataService : ISettingsDataService
{
    private const string apiBaseDefaultUrl= "http://192.168.20.168:1234";
    public async Task<string> GetApiUrlAsync()
    {
        string url = Preferences.Get("apiUrl",apiBaseDefaultUrl );
        
        return await Task.FromResult( url);
    }

    public string GetApiUrl()
    {
        string url = Preferences.Get("apiUrl", apiBaseDefaultUrl);
        return url;
    }
    public Task<int> SetApiUrlAsync(string url)
    {
        Preferences.Set("apiUrl",url);
        return Task.FromResult(0);
    }
    public int SetApiUrl(string url)
    {
        Preferences.Set("apiUrl",url);
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