using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class SyncItemsPageModel : ObservableObject
{
    private readonly LocalItemsRepo _localItemsRepo;
    private readonly LocalBuyDocumentsRepo _localBuyDocsRepo;
    private readonly IBusinessServerDataAccess _businessServerDataAccess;
    private readonly ILogger<SyncItemsPageModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    [ObservableProperty] private bool _isProgressBarVisible = false;
    [ObservableProperty] private int _operationProgress = 0;
    [ObservableProperty] private bool _isWaitingForResponse = false;
   
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddItemsToLocalDatabaseCommand))]
    private bool _canSyncItems = false;

    [ObservableProperty] private ICollection<ItemListDto> _items = new List<ItemListDto>();
    [ObservableProperty] private int _itemCount = 0;
    
    [ObservableProperty] private ICollection<SupplierListDto> _suppliers = new List<SupplierListDto>();
    [ObservableProperty] private int _supplierCount = 0;

    [ObservableProperty] private ICollection<BuyDocListDto> _buyDocs = new List<BuyDocListDto>();
    [ObservableProperty] private int _buyDocsCount = 0;

    [ObservableProperty] private ICollection<BuyDocLineListDto> _buyDocLines = new List<BuyDocLineListDto>();
    [ObservableProperty] private int _buyDocLinesCount = 0;

    public ObservableCollection<LogEntry> LogEntries { get; } = new();
    [ObservableProperty]
    private int _lastLogEntryIndex;

    private void AddLog(string message)
    {
        //LogEntries.Add(new LogEntry { Timestamp = DateTime.Now.ToString("HH:mm:ss"), Message = message });
        LogEntries.Insert(0, new LogEntry { Timestamp = DateTime.Now.ToString("HH:mm:ss"), Message = message });
    }

    public SyncItemsPageModel(LocalItemsRepo localItemsRepo,LocalBuyDocumentsRepo localBuyDocsRepo ,IBusinessServerDataAccess businessServerDataAccess,
        ILogger<SyncItemsPageModel> logger)
    {
        _localItemsRepo = localItemsRepo;
        _localBuyDocsRepo = localBuyDocsRepo;
        _businessServerDataAccess = businessServerDataAccess;
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
        IsWaitingForResponse = false;
    }

    [RelayCommand]
    private async Task GetItems()
    {
        await GetItemsProcessAsync();
    }

    private async Task GetItemsProcessAsync()
    {
        AddLog("Getting items from server");
        try
        {
            IsWaitingForResponse = true;
            Items = await _businessServerDataAccess.GetBusinessServerItemsListAsync();
            ItemCount = Items.Count;
            AddLog($"Connected and retrieved {ItemCount} items");
            // await AppShell.DisplayToastAsync("Connected and retrieved items");
        }
        catch (Exception e)
        {
            
            _logger.LogError(e, "Error in GetItems");
            AddLog($"Error in GetItems: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetItems: {e.Message}");
        }
        finally
        {
            IsWaitingForResponse = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSyncItems))]
    private async Task AddItemsToLocalDatabase()
    {
        await AddOrUpdateItemsToLocalDatabaseProcess();
    }

    private async Task AddOrUpdateItemsToLocalDatabaseProcess()
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
        //var currentPage = Application.Current?.MainPage;
        foreach (var item in Items)
        {
            try
            {
                //Check if item with Id already exists in the database
                if (await _localItemsRepo.ItemExist(item.Id))
                {
                    var result = await _localItemsRepo.UpdateItemAsync(item);
                    AddLog($"Index: {index + 1} Upd Id:{item.Id}, Name: {item.Name} returned {result.ToString()}");
                    updatedCount++;
                }
                else
                {
                    var result = await _localItemsRepo.AddItemAsync(item);
                    AddLog($"Index: {index + 1} Add Id:{item.Id}, Name: {item.Name} returned {result}");
                    addedCount++;
                }
                ((IProgress<int>)progress).Report(++index);
                // Allow time for UI to update after each iteration
                if (index % 10 == 0)
                {
                    LastLogEntryIndex = index;
                    await Task.Delay(50); // Introduce a small delay for smoother progress visualization    
                }
              
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
        Preferences.Default.Set("last_synced", DateTime.Now);
        await AppShell.DisplayToastAsync("Finished updating local database");
    }
}