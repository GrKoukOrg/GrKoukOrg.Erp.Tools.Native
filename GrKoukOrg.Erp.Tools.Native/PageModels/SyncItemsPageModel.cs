using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class SyncItemsPageModel: ObservableObject
{
    private readonly LocalItemsRepo _localItemsRepo;
    private readonly IServerDataAccess _serverDataAccess;
    private readonly IHttpClientFactory _httpClientFactory;

    [ObservableProperty]
    private bool _isConnected = false;
    
    [ObservableProperty]
    private ICollection<ItemListDto> _items = new List<ItemListDto>();

    [ObservableProperty]
    private int _itemCount = 0;

    public SyncItemsPageModel(LocalItemsRepo localItemsRepo,IServerDataAccess serverDataAccess)
    {
        _localItemsRepo = localItemsRepo;
        _serverDataAccess = serverDataAccess;
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
}