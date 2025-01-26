using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class SyncItemsPageModel: ObservableObject
{
    private readonly LocalItemsRepo _localItemsRepo;
    private readonly IServerDataAccess _serverDataAccess;
    private readonly ILogger<SyncItemsPageModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    [ObservableProperty]
    private bool _isConnected = false;
    
    [ObservableProperty]
    private ICollection<ItemListDto> _items = new List<ItemListDto>();

    [ObservableProperty]
    private int _itemCount = 0;

    public SyncItemsPageModel(LocalItemsRepo localItemsRepo,IServerDataAccess serverDataAccess, ILogger<SyncItemsPageModel> logger)
    {
        _localItemsRepo = localItemsRepo;
        _serverDataAccess = serverDataAccess;
        _logger = logger;
    }
    
    [RelayCommand]
    private async Task Appearing()
    {
        //Projects = await _projectRepository.ListAsync();
        
    }

    [RelayCommand]
    private async Task GetItems()
    {
        try
        {
            Items = await _serverDataAccess.GetServerItemsListAsync();
            ItemCount = Items.Count;
            await AppShell.DisplayToastAsync("Connected and retrieved items");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetItems");
            await AppShell.DisplayToastAsync($"There was an error: {e.Message}");
            
        }
       
       
    }

    [RelayCommand]
    private async Task AddItemsToLocalDatabase()
    {
       
        foreach (var (item, index) in Items.Select((item, index) => (item, index)))
        {
            var result = await _localItemsRepo.AddItemAsync(item);
            _logger.LogDebug($"Index: {index}, Id: {item.Id}, Name: {item.Name} result returned {result}");
            
        }
        await AppShell.DisplayToastAsync("Items added to local database");
    }
}