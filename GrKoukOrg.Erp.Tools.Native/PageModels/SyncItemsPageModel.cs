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

        Items = await _serverDataAccess.GetServerItemsListAsync();
        ItemCount = Items.Count;
    }

    [RelayCommand]
    private async Task AddItemsToLocalDatabase()
    {
        foreach (var item in Items)
        {
            //ItemListDto itemToAdd =
            _logger.LogDebug(item.Id.ToString() + " " + item.Name );
            _localItemsRepo.AddItemAsync(item);
           // _localItemsRepo.SaveItemAsync(item);
        }
    }
}