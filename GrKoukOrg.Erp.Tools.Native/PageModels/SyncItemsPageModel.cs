using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class SyncItemsPageModel : ObservableObject
{
    private readonly LocalItemsRepo _localItemsRepo;
    private readonly IServerDataAccess _serverDataAccess;
    private readonly ILogger<SyncItemsPageModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    [ObservableProperty] private bool _isProgressBarVisible = false;
    [ObservableProperty] private int _operationProgress = 0;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddItemsToLocalDatabaseCommand))]
    private bool _canSyncItems = false;

    [ObservableProperty] private ICollection<ItemListDto> _items = new List<ItemListDto>();

    [ObservableProperty] private int _itemCount = 0;
    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    private void AddLog(string message)
    {
        LogEntries.Add(new LogEntry { Timestamp = DateTime.Now.ToString("HH:mm:ss"), Message = message });
    }

    public SyncItemsPageModel(LocalItemsRepo localItemsRepo, IServerDataAccess serverDataAccess,
        ILogger<SyncItemsPageModel> logger)
    {
        _localItemsRepo = localItemsRepo;
        _serverDataAccess = serverDataAccess;
        _logger = logger;
    }

    partial void OnItemCountChanged(int value)
    {
        CanSyncItems = value > 0;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        //Projects = await _projectRepository.ListAsync();
        //OperationProgress = 50;
        IsProgressBarVisible = false;
    }

    [RelayCommand]
    private async Task GetItems()
    {
        AddLog("Getting items from server");
        try
        {
            Items = await _serverDataAccess.GetServerItemsListAsync();
            ItemCount = Items.Count;
            AddLog($"Connected and retrieved {ItemCount} items");
            // await AppShell.DisplayToastAsync("Connected and retrieved items");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetItems");
            AddLog($"Error in GetItems: {e.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSyncItems))]
    private async Task AddItemsToLocalDatabase()
    {
        AddLog("Adding items to local database");

        foreach (var (item, index) in Items.Select((item, index) => (item, index)))
        {
            //Check if item with Id already exists in the database
            if (await _localItemsRepo.ItemExist(item.Id))
            {
                var result = await _localItemsRepo.UpdateItemAsync(item);
                AddLog($"Index: {index}, Id: {item.Id}, Name: {item.Name} Update item result returned {result}");
                
            }
            else
            {
                var result = await _localItemsRepo.AddItemAsync(item);
                AddLog($"Index: {index}, Id: {item.Id}, Name: {item.Name} Add item result returned {result}");
            }
            OperationProgress=index;
        }

        await AppShell.DisplayToastAsync("Items added and updated to local database");
    }
}