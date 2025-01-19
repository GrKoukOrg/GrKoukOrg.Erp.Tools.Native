using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Services;
using Microsoft.Maui.Storage;

public interface ISettingsDataService
{
    Task<string> GetApiUrl();
    Task<int> SetApiUrl(string url);
}

public class SettingsMemoryDataService : ISettingsDataService
{
    public async Task<string> GetApiUrl()
    {
        string url = Preferences.Get("apiUrl", "http://192.168.90.100:1234");
        
        return await Task.FromResult( url);
    }

    public Task<int> SetApiUrl(string url)
    {
        Preferences.Set("apiUrl",url);
        return Task.FromResult(0);
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