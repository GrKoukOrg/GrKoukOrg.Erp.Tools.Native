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
    private readonly LocalSuppliersRepo _localSuppliersRepo;
    private readonly LocalBuyDocLinesRepo _localBuyDocLinesRepo;
    private readonly IBusinessServerDataAccess _businessServerDataAccess;
    private readonly LocalCustomerRepo _localCustomerRepo;
    private readonly LocalSaleDocumentsRepo _localSaleDocumentsRepo;
    private readonly LocalSalesDocLinesRepo _localSalesDocLinesRepo;
    private readonly ILogger<SyncItemsPageModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    [ObservableProperty] private bool _isProgressBarVisible = false;
    [ObservableProperty] private int _operationProgress = 0;
    [ObservableProperty] private bool _isWaitingForResponse = false;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddItemsToLocalDatabaseCommand))]
    private bool _canSyncItems = false;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddSuppliersToLocalDatabaseCommand))]
    private bool _canSyncSuppliers = false;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddBuyDocsToLocalDatabaseCommand))]
    private bool _canSyncBuyDocs = false;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddBuyDocLinesToLocalDatabaseCommand))]
    private bool _canSyncBuyDocLines = false;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddCustomersToLocalDatabaseCommand))]
    private bool _canSyncCustomers = false;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddSaleDocsToLocalDatabaseCommand))]
    private bool _canSyncSaleDocs = false;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddSaleDocLinesToLocalDatabaseCommand))]
    private bool _canSyncSaleDocLines = false;

    [ObservableProperty] private ICollection<ItemListDto> _items = new List<ItemListDto>();
    [ObservableProperty] private int _itemCount = 0;

    [ObservableProperty] private ICollection<SupplierListDto> _suppliers = new List<SupplierListDto>();
    [ObservableProperty] private int _supplierCount = 0;

    [ObservableProperty] private ICollection<BuyDocListDto> _buyDocs = new List<BuyDocListDto>();
    [ObservableProperty] private int _buyDocsCount = 0;

    [ObservableProperty] private ICollection<BuyDocLineListDto> _buyDocLines = new List<BuyDocLineListDto>();
    [ObservableProperty] private int _buyDocLinesCount = 0;

    [ObservableProperty] private ICollection<CustomerListDto> _customers = new List<CustomerListDto>();
    [ObservableProperty] private int _customerCount = 0;

    [ObservableProperty] private ICollection<SaleDocListDto> _saleDocs = new List<SaleDocListDto>();
    [ObservableProperty] private int _saleDocsCount = 0;

    [ObservableProperty] private ICollection<SaleDocLineListDto> _saleDocLines = new List<SaleDocLineListDto>();
    [ObservableProperty] private int _saleDocLinesCount = 0;
    public ObservableCollection<LogEntry> LogEntries { get; } = new();
    [ObservableProperty] private int _lastLogEntryIndex;

    private void AddLog(string message)
    {
        //LogEntries.Add(new LogEntry { Timestamp = DateTime.Now.ToString("HH:mm:ss"), Message = message });
        LogEntries.Insert(0, new LogEntry { Timestamp = DateTime.Now.ToString("HH:mm:ss"), Message = message });
    }

    public SyncItemsPageModel(LocalItemsRepo localItemsRepo, LocalBuyDocumentsRepo localBuyDocsRepo,
        LocalSuppliersRepo localSuppliersRepo, LocalBuyDocLinesRepo localBuyDocLinesRepo,
        IBusinessServerDataAccess businessServerDataAccess,
        LocalCustomerRepo localCustomerRepo,
        LocalSaleDocumentsRepo localSaleDocumentsRepo,
        LocalSalesDocLinesRepo localSalesDocLinesRepo,
        ILogger<SyncItemsPageModel> logger)
    {
        _localItemsRepo = localItemsRepo;
        _localBuyDocsRepo = localBuyDocsRepo;
        _localSuppliersRepo = localSuppliersRepo;
        _localBuyDocLinesRepo = localBuyDocLinesRepo;
        _businessServerDataAccess = businessServerDataAccess;
        _localCustomerRepo = localCustomerRepo;
        _localSaleDocumentsRepo = localSaleDocumentsRepo;
        _localSalesDocLinesRepo = localSalesDocLinesRepo;
        _logger = logger;
    }

    partial void OnItemCountChanged(int value)
    {
        CanSyncItems = value > 0;
    }

    partial void OnSupplierCountChanged(int value)
    {
        CanSyncSuppliers = value > 0;
    }

    partial void OnBuyDocsCountChanged(int value)
    {
        CanSyncBuyDocs = value > 0;
    }

    partial void OnBuyDocLinesCountChanged(int value)
    {
        CanSyncBuyDocLines = value > 0;
    }

    partial void OnCustomerCountChanged(int value)
    {
        CanSyncCustomers = value > 0;
    }

    partial void OnSaleDocsCountChanged(int value)
    {
        CanSyncSaleDocs = value > 0;
    }

    partial void OnSaleDocLinesCountChanged(int value)
    {
        CanSyncSaleDocLines = value > 0;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        IsProgressBarVisible = false;
        IsWaitingForResponse = false;
    }

    [RelayCommand]
    private async Task GetItems()
    {
        await GetItemsProcessAsync();
    }

    [RelayCommand]
    private async Task GetSuppliers()
    {
        await GetSuppliersProcessAsync();
    }

    [RelayCommand]
    private async Task GetBuyDocs()
    {
        await GetBuyDocumentsProcessAsync();
    }

    [RelayCommand]
    private async Task GetBuyDocLines()
    {
        await GetBuyDocLinesProcessAsync();
    }
    [RelayCommand]
    private async Task GetCustomers()
    {
        await GetCustomersProcessAsync();
    }

    [RelayCommand]
    private async Task GetSaleDocs()
    {
        await GetSaleDocumentsProcessAsync();
    }

    [RelayCommand]
    private async Task GetSaleDocLines()
    {
        await GetSaleDocLinesProcessAsync();
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
            _logger.LogError(e, "Error in GetItemsProcessAsync");
            AddLog($"Error in GetItems: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetItems: {e.Message}");
        }
        finally
        {
            IsWaitingForResponse = false;
        }
    }

    private async Task GetSuppliersProcessAsync()
    {
        AddLog("Getting Suppliers from server");
        try
        {
            IsWaitingForResponse = true;
            Suppliers = await _businessServerDataAccess.GetBusinessServerSupplierListAsync();
            SupplierCount = Suppliers.Count;
            AddLog($"Connected and retrieved {SupplierCount} suppliers");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetSuppliersProcessAsync");
            AddLog($"Error in GetSuppliersProcessAsync: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetSuppliersProcessAsync: {e.Message}");
        }
        finally
        {
            IsWaitingForResponse = false;
        }
    }

    private async Task GetCustomersProcessAsync()
    {
        AddLog("Getting Customers from server");
        try
        {
            IsWaitingForResponse = true;
            Customers = await _businessServerDataAccess.GetBusinessServerCustomerListAsync();
            CustomerCount = Customers.Count;
            AddLog($"Connected and retrieved {CustomerCount} customers");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetCustomersProcessAsync");
            AddLog($"Error in GetCustomersProcessAsync: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetCustomersProcessAsync: {e.Message}");
        }
        finally
        {
            IsWaitingForResponse = false;
        }
    }

    private async Task GetBuyDocumentsProcessAsync()
    {
        AddLog("Getting Buy Documents from server");
        try
        {
            IsWaitingForResponse = true;
            BuyDocs = await _businessServerDataAccess.GetBusinessServerBuyDocListAsync();
            BuyDocsCount = BuyDocs.Count;
            AddLog($"Connected and retrieved {BuyDocsCount} Buy Documents");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetBuyDocumentsProcessAsync");
            AddLog($"Error in GetBuyDocumentsProcessAsync: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetBuyDocumentsProcessAsync: {e.Message}");
        }
        finally
        {
            IsWaitingForResponse = false;
        }
    }

    private async Task GetSaleDocumentsProcessAsync()
    {
        AddLog("Getting Sale Documents from server");
        try
        {
            IsWaitingForResponse = true;
            SaleDocs = await _businessServerDataAccess.GetBusinessServerSaleDocListAsync();
            SaleDocsCount = SaleDocs.Count;
            AddLog($"Connected and retrieved {SaleDocsCount} Sale Documents");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetSaleDocumentsProcessAsync");
            AddLog($"Error in GetSaleDocumentsProcessAsync: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetSaleDocumentsProcessAsync: {e.Message}");
        }
        finally
        {
            IsWaitingForResponse = false;
        }
    }

    private async Task GetBuyDocLinesProcessAsync()
    {
        AddLog("Getting Buy Doc Lines from server");
        try
        {
            IsWaitingForResponse = true;
            BuyDocLines = await _businessServerDataAccess.GetBusinessServerBuyDocLineListAsync();
            BuyDocLinesCount = BuyDocLines.Count;
            AddLog($"Connected and retrieved {BuyDocLinesCount} Buy Documents");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetBuyDocLinesProcessAsync");
            AddLog($"Error in GetBuyDocLinesProcessAsync: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetBuyDocLinesProcessAsync: {e.Message}");
        }
        finally
        {
            IsWaitingForResponse = false;
        }
    }

    private async Task GetSaleDocLinesProcessAsync()
    {
        AddLog("Getting Sale Doc Lines from server");
        try
        {
            IsWaitingForResponse = true;
            SaleDocLines = await _businessServerDataAccess.GetBusinessServerSaleDocLineListAsync();
            SaleDocLinesCount = SaleDocLines.Count;
            AddLog($"Connected and retrieved {SaleDocLinesCount} Sale Documents");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetSaleDocLinesProcessAsync");
            AddLog($"Error in GetSaleDocLinesProcessAsync: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetSaleDocLinesProcessAsync: {e.Message}");
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

    [RelayCommand(CanExecute = nameof(CanSyncSuppliers))]
    private async Task AddSuppliersToLocalDatabase()
    {
        await AddOrUpdateSuppliersToLocalDatabaseProcess();
    }

    [RelayCommand(CanExecute = nameof(CanSyncCustomers))]
    private async Task AddCustomersToLocalDatabase()
    {
        await AddOrUpdateCustomersToLocalDatabaseProcess();
    }

    [RelayCommand(CanExecute = nameof(CanSyncBuyDocs))]
    private async Task AddBuyDocsToLocalDatabase()
    {
        await AddOrUpdateBuyDocsToLocalDatabaseProcess();
    }

    [RelayCommand(CanExecute = nameof(CanSyncSaleDocs))]
    private async Task AddSaleDocsToLocalDatabase()
    {
        await AddOrUpdateSaleDocsToLocalDatabaseProcess();
    }

    [RelayCommand(CanExecute = nameof(CanSyncBuyDocLines))]
    private async Task AddBuyDocLinesToLocalDatabase()
    {
        await AddOrUpdateBuyDocLinesToLocalDatabaseProcess();
    }

    [RelayCommand(CanExecute = nameof(CanSyncSaleDocLines))]
    private async Task AddSaleDocLinesToLocalDatabase()
    {
        await AddOrUpdateSaleDocLinesToLocalDatabaseProcess();
    }

    [RelayCommand]
    private async Task DeleteAllItemsFromLocalDatabase()
    {
        AddLog("Delete all items from local database");
        var result = await _localItemsRepo.DeleteAllItemsAsync();
        AddLog($"Deleted {result.ToString()} items");
    }
    [RelayCommand]
    private async Task DropBuyDocLineTableFromLocalDatabase()
    {
        AddLog("Drop buydoclines table from local database");
        var result = await _localBuyDocLinesRepo.DropBuyDocLinesTableAsync();
        AddLog($"Dropped table");
    }
    [RelayCommand]
    private async Task DropSaleDocLineTableFromLocalDatabase()
    {
        AddLog("Drop saledoclines table from local database");
        var result = await _localSalesDocLinesRepo.DropSaleDocLinesTableAsync();
        AddLog($"Dropped table");
    }
    [RelayCommand]
    private async Task DeleteAllSuppliersFromLocalDatabase()
    {
        AddLog("Delete all suppliers from local database");
        var result = await _localSuppliersRepo.DeleteAllSuppliersAsync();
        AddLog($"Deleted {result.ToString()} suppliers");
    }

    [RelayCommand]
    private async Task DeleteAllCustomersFromLocalDatabase()
    {
        AddLog("Delete all customers from local database");
        var result = await _localCustomerRepo.DeleteAllCustomersAsync();
        AddLog($"Deleted {result.ToString()} customers");
    }

    [RelayCommand]
    private async Task DeleteAllBuyDocumentsFromLocalDatabase()
    {
        AddLog("Delete all Buy Documets from local database");
        var result = await _localBuyDocsRepo.DeleteAllBuyDocumentAsync();
        AddLog($"Deleted {result.ToString()} buy documents");
    }

    [RelayCommand]
    private async Task DeleteAllSaleDocumentsFromLocalDatabase()
    {
        AddLog("Delete all Sale Documets from local database");
        var result = await _localSaleDocumentsRepo.DeleteAllSaleDocumentAsync();
        AddLog($"Deleted {result.ToString()} sale documents");
    }

    [RelayCommand]
    private async Task DeleteAllBuyDocLinesFromLocalDatabase()
    {
        AddLog("Delete all Buy Doc Lines from local database");
        var result = await _localBuyDocLinesRepo.DeleteAllBuyDocLineAsync();
        AddLog($"Deleted {result.ToString()} buy doc lines");
    }

    [RelayCommand]
    private async Task DeleteAllSaleDocLinesFromLocalDatabase()
    {
        AddLog("Delete all Sale Doc Lines from local database");
        var result = await _localSalesDocLinesRepo.DeleteAllSaleDocLineAsync();
        AddLog($"Deleted {result.ToString()} sale doc lines");
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
        ItemCount = totalCount;
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
                if (index % 20 == 0)
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

    private async Task AddOrUpdateSuppliersToLocalDatabaseProcess()
    {
        AddLog("Adding suppliers to local database");
        // Make the progress bar visible at the start
        IsProgressBarVisible = true;

        // Create progress reporter to handle UI updates
        var progress = new Progress<int>(value =>
        {
            OperationProgress = value;
            OnPropertyChanged(nameof(OperationProgress));
        });
        int index = 0;
        int totalCount = Suppliers.Count;
        ItemCount = totalCount;
        int updatedCount = 0;
        int addedCount = 0;
        //var currentPage = Application.Current?.MainPage;
        foreach (var item in Suppliers)
        {
            try
            {
                //Check if item with Id already exists in the database
                if (await _localSuppliersRepo.SupplierExist(item.Id))
                {
                    var result = await _localSuppliersRepo.UpdateSupplierAsync(item);
                    AddLog($"Index: {index + 1} Upd Id:{item.Id}, Name: {item.Name} returned {result.ToString()}");
                    updatedCount++;
                }
                else
                {
                    var result = await _localSuppliersRepo.AddSupplierAsync(item);
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

        AddLog("All Suppliers have been processed.");
        AddLog("Added: " + addedCount + ", Updated: " + updatedCount + "");
        IsProgressBarVisible = false; // Hide the progress bar after completing the operation
        Preferences.Default.Set("last_synced", DateTime.Now);
        await AppShell.DisplayToastAsync("Finished updating local database");
    }

    private async Task AddOrUpdateCustomersToLocalDatabaseProcess()
    {
        AddLog("Adding Customers to local database");
        // Make the progress bar visible at the start
        IsProgressBarVisible = true;

        // Create progress reporter to handle UI updates
        var progress = new Progress<int>(value =>
        {
            OperationProgress = value;
            OnPropertyChanged(nameof(OperationProgress));
        });
        int index = 0;
        int totalCount = Customers.Count;
        ItemCount = totalCount;
        int updatedCount = 0;
        int addedCount = 0;
        //var currentPage = Application.Current?.MainPage;
        foreach (var item in Customers)
        {
            try
            {
                //Check if item with Id already exists in the database
                if (await _localCustomerRepo.CustomerExist(item.Id))
                {
                    var result = await _localCustomerRepo.UpdateCustomerAsync(item);
                    AddLog($"Index: {index + 1} Upd Id:{item.Id}, Name: {item.Name} returned {result.ToString()}");
                    updatedCount++;
                }
                else
                {
                    var result = await _localCustomerRepo.AddCustomerAsync(item);
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

        AddLog("All Customers have been processed.");
        AddLog("Added: " + addedCount + ", Updated: " + updatedCount + "");
        IsProgressBarVisible = false; // Hide the progress bar after completing the operation
        Preferences.Default.Set("last_synced", DateTime.Now);
        await AppShell.DisplayToastAsync("Finished updating local database");
    }

    private async Task AddOrUpdateBuyDocsToLocalDatabaseProcess()
    {
        AddLog("Adding Buy Documents to local database");
        // Make the progress bar visible at the start
        IsProgressBarVisible = true;

        // Create progress reporter to handle UI updates
        var progress = new Progress<int>(value =>
        {
            OperationProgress = value;
            OnPropertyChanged(nameof(OperationProgress));
        });
        int index = 0;
        int totalCount = BuyDocs.Count;
        ItemCount= totalCount;
        int updatedCount = 0;
        int addedCount = 0;
        //var currentPage = Application.Current?.MainPage;
        foreach (var item in BuyDocs)
        {
            try
            {
                //Check if item with Id already exists in the database
                if (await _localBuyDocsRepo.BuyDocumentExist(item.Id))
                {
                    var result = await _localBuyDocsRepo.UpdateBuyDocumentAsync(item);
                    AddLog(
                        $"Index: {index + 1} Upd Id:{item.Id}, Name: {item.SupplierName} returned {result.ToString()}");
                    updatedCount++;
                }
                else
                {
                    var result = await _localBuyDocsRepo.AddBuyDocumentAsync(item);
                    AddLog($"Index: {index + 1} Add Id:{item.Id}, Name: {item.SupplierName} returned {result}");
                    addedCount++;
                }

                ((IProgress<int>)progress).Report(++index);
                // Allow time for UI to update after each iteration
                if (index % 20 == 0)
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

        AddLog("All Buy documents have been processed.");
        AddLog("Added: " + addedCount + ", Updated: " + updatedCount + "");
        IsProgressBarVisible = false; // Hide the progress bar after completing the operation
        Preferences.Default.Set("last_synced", DateTime.Now);
        await AppShell.DisplayToastAsync("Finished updating local database");
    }

    private async Task AddOrUpdateSaleDocsToLocalDatabaseProcess()
    {
        AddLog("Adding Sale Documents to local database");
        // Make the progress bar visible at the start
        IsProgressBarVisible = true;

        // Create progress reporter to handle UI updates
        var progress = new Progress<int>(value =>
        {
            OperationProgress = value;
            OnPropertyChanged(nameof(OperationProgress));
        });
        int index = 0;
        int totalCount = SaleDocs.Count;
        ItemCount = totalCount;
        int updatedCount = 0;
        int addedCount = 0;
        //var currentPage = Application.Current?.MainPage;
        foreach (var item in SaleDocs)
        {
            try
            {
                //Check if item with Id already exists in the database
                if (await _localSaleDocumentsRepo.SaleDocumentExist(item.Id))
                {
                    var result = await _localSaleDocumentsRepo.UpdateSaleDocumentAsync(item);
                    AddLog(
                        $"Index: {index + 1} Upd Id:{item.Id}, Name: {item.CustomerName} returned {result.ToString()}");
                    updatedCount++;
                }
                else
                {
                    var result = await _localSaleDocumentsRepo.AddSaleDocumentAsync(item);
                    AddLog($"Index: {index + 1} Add Id:{item.Id}, Name: {item.CustomerName} returned {result}");
                    addedCount++;
                }

                ((IProgress<int>)progress).Report(++index);
                // Allow time for UI to update after each iteration
                if (index % 20 == 0)
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

        AddLog("All Sale documents have been processed.");
        AddLog("Added: " + addedCount + ", Updated: " + updatedCount + "");
        IsProgressBarVisible = false; // Hide the progress bar after completing the operation
        Preferences.Default.Set("last_synced", DateTime.Now);
        await AppShell.DisplayToastAsync("Finished updating local database");
    }

    private async Task AddOrUpdateBuyDocLinesToLocalDatabaseProcess()
    {
        AddLog("Adding Buy Doc Lines to local database");
        // Make the progress bar visible at the start
        IsProgressBarVisible = true;

        // Create progress reporter to handle UI updates
        var progress = new Progress<int>(value =>
        {
            OperationProgress = value;
            OnPropertyChanged(nameof(OperationProgress));
        });
        int index = 0;
        int totalCount = BuyDocLines.Count;
        ItemCount = totalCount;
        int updatedCount = 0;
        int addedCount = 0;
        //var currentPage = Application.Current?.MainPage;
        foreach (var item in BuyDocLines)
        {
            try
            {
                //Check if item with Id already exists in the database
                if (await _localBuyDocLinesRepo.BuyDocLineExist(item.Id))
                {
                    var result = await _localBuyDocLinesRepo.UpdateBuyDocLineAsync(item);
                    AddLog($"Index: {index + 1} Upd Id:{item.Id}, Name: {item.ItemName} returned {result.ToString()}");
                    updatedCount++;
                }
                else
                {
                    var result = await _localBuyDocLinesRepo.AddBuyDocLineAsync(item);
                    AddLog($"Index: {index + 1} Add Id:{item.Id}, Name: {item.ItemName} returned {result}");
                    addedCount++;
                }

                ((IProgress<int>)progress).Report(++index);
                // Allow time for UI to update after each iteration
                if (index % 20 == 0)
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

        AddLog("All Buy Doc Lines have been processed.");
        AddLog("Added: " + addedCount + ", Updated: " + updatedCount + "");
        IsProgressBarVisible = false; // Hide the progress bar after completing the operation
        Preferences.Default.Set("last_synced", DateTime.Now);
        await AppShell.DisplayToastAsync("Finished updating local database");
    }

    private async Task AddOrUpdateSaleDocLinesToLocalDatabaseProcess()
    {
        AddLog("Adding Sale Doc Lines to local database");
        // Make the progress bar visible at the start
        IsProgressBarVisible = true;

        // Create progress reporter to handle UI updates
        var progress = new Progress<int>(value =>
        {
            OperationProgress = value;
            OnPropertyChanged(nameof(OperationProgress));
        });
        int index = 0;
        int totalCount = SaleDocLines.Count;
        ItemCount = totalCount;
        int updatedCount = 0;
        int addedCount = 0;
        //var currentPage = Application.Current?.MainPage;
        foreach (var item in SaleDocLines)
        {
            try
            {
                //Check if item with Id already exists in the database
                if (await _localSalesDocLinesRepo.SaleDocLineExist(item.Id))
                {
                    var result = await _localSalesDocLinesRepo.UpdateSaleDocLineAsync(item);
                    AddLog($"Index: {index + 1} Upd Id:{item.Id}, Name: {item.ItemName} returned {result.ToString()}");
                    updatedCount++;
                }
                else
                {
                    var result = await _localSalesDocLinesRepo.AddSaleDocLineAsync(item);
                    AddLog($"Index: {index + 1} Add Id:{item.Id}, Name: {item.ItemName} returned {result}");
                    addedCount++;
                }

                ((IProgress<int>)progress).Report(++index);
                // Allow time for UI to update after each iteration
                if (index % 20 == 0)
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

        AddLog("All Sale Doc Lines have been processed.");
        AddLog("Added: " + addedCount + ", Updated: " + updatedCount + "");
        IsProgressBarVisible = false; // Hide the progress bar after completing the operation
        Preferences.Default.Set("last_synced", DateTime.Now);
        await AppShell.DisplayToastAsync("Finished updating local database");
    }
}