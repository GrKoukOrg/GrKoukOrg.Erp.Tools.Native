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
        // Make the progress bar visible at the start
        IsProgressBarVisible = true;

        // Create progress reporter to handle UI updates
        var progress = new Progress<int>(value =>
        {
            OperationProgress = value;
            OnPropertyChanged(nameof(OperationProgress));
        });
        int index = 0;
        int totalCount = Items.Count;
        int updatedCount = 0;
        int addedCount = 0;
        foreach (var item in Items)
        {
            try
            {
                //Check if item with Id already exists in the database
                if (await _localItemsRepo.ItemExist(item.Id))
                {
                    var result = await _localItemsRepo.UpdateItemAsync(item);
                    AddLog($"Index: {index + 1} / {totalCount}. Update item->Id: {item.Id}, Name: {item.Name}  result returned {result}");
                    updatedCount++;
                }
                else
                {
                    var result = await _localItemsRepo.AddItemAsync(item);
                    AddLog($"Index: {index + 1}/{totalCount}. Add item->Id: {item.Id}, Name: {item.Name} result returned {result}");
                    addedCount++;
                }
                ((IProgress<int>)progress).Report(++index);
                // Allow time for UI to update after each iteration
                if (index % 10 == 0)
                {
                    await Task.Delay(20); // Introduce a small delay for smoother progress visualization    
                }
                

                // // Ensure that LogEntries are updated immediately *** This code did not work *** 
                // await MainThread.InvokeOnMainThreadAsync(() =>
                // {
                //     OnPropertyChanged(nameof(LogEntries));
                // });
                
            }
            catch (Exception ex)
            {
                AddLog($"Error processing item {item.Id}: {ex.Message}");
                _logger.LogError(ex, $"Error processing item {item.Id}");
            }
        }
        AddLog("All items have been processed.");
        AddLog("Added: " + addedCount + ", Updated: " + updatedCount + "");
        IsProgressBarVisible = false; // Hide the progress bar after completing the operation

        await AppShell.DisplayToastAsync("Finished updating local database");
    }
}