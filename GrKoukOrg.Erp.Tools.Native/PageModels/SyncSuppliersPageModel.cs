using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using GrKoukOrg.Erp.Tools.Native.Shared;
using Syncfusion.Maui.ListView;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class SyncSuppliersPageModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly ISettingsDataService _settingsDataService;
    private readonly IBusinessServerDataAccess _businessServerDataAccess;
    private string _companyCode;
    private SupplierListDto _selectedBusinessSupplier;
    private SupplierListDto _selectedErpSupplier;
    private MatchedSuppliersDto _selectedMatchedSupplier;

    [ObservableProperty] private bool _isWaitingForResponse = false;

    [ObservableProperty]
    private ObservableCollection<SupplierListDto> _suppliers =
        new ObservableCollection<SupplierListDto>(new List<SupplierListDto>());

    [ObservableProperty]
    private ObservableCollection<SupplierListDto> _erpSuppliers =
        new ObservableCollection<SupplierListDto>(new List<SupplierListDto>());

    [ObservableProperty] private ObservableCollection<SupplierListDto> _erpSelectedSuppliers =
        new ObservableCollection<SupplierListDto>(new List<SupplierListDto>());

    [ObservableProperty] private ObservableCollection<MatchedSuppliersDto> _matchedSuppliers =
        new ObservableCollection<MatchedSuppliersDto>(new List<MatchedSuppliersDto>());

    [ObservableProperty] private int _supplierCount = 0;
    [ObservableProperty] private int _erpSupplierCount = 0;

    public ObservableCollection<LogEntry> LogEntries { get; } = new();
    [ObservableProperty] private int _lastLogEntryIndex;

    public SyncSuppliersPageModel(ApiService apiService, ISettingsDataService settingsDataService,
        IBusinessServerDataAccess businessServerDataAccess)
    {
        _apiService = apiService;
        _settingsDataService = settingsDataService;
        _businessServerDataAccess = businessServerDataAccess;
    }

    private void AddLog(string message)
    {
        //LogEntries.Add(new LogEntry { Timestamp = DateTime.Now.ToString("HH:mm:ss"), Message = message });
        LogEntries.Insert(0, new LogEntry { Timestamp = DateTime.Now.ToString("HH:mm:ss"), Message = message });
    }

    [RelayCommand]
    private async Task Appearing()
    {
        await Task.Run(() =>
        {
            // IsProgressBarVisible = false;
            IsWaitingForResponse = false;
            _companyCode = _settingsDataService.GetBusinessCompanyCode();
        });
    }

    [RelayCommand]
    private async Task GetSuppliers()
    {
        await GetSuppliersProcessAsync();
    }

    [RelayCommand]
    private async Task SendMatchedSuppliersToErp()
    {
        var busItemsRequest = new SyncBusinessEntityRequest<SyncSupplierDto>();
        busItemsRequest.CompanyCode=_companyCode;
        foreach (var matchedItem in MatchedSuppliers)
        { 
            var erpItem=matchedItem.ErpSupplier;
            var busItem = matchedItem.BusinessSupplier;
            //Create update item
            busItemsRequest.Items.Add(new SyncSupplierDto()
            {
                BusId = matchedItem.BusinessSupplier.Id,
                ErpId = matchedItem.ErpSupplier.Id,
                BusCode = matchedItem.BusinessSupplier.Code,
                CompanyCode = _companyCode,
                Name = matchedItem.BusinessSupplier.Name,
                TaxNumber = matchedItem.BusinessSupplier.Afm,
                SourceChecksum = ChecksumHelper.CalculateChecksum(matchedItem.BusinessSupplier.Id.ToString()
                    , matchedItem.BusinessSupplier.Code, matchedItem.BusinessSupplier.Name, matchedItem.BusinessSupplier.Afm, _companyCode
                )

            });
            
        }
        //Send request to Erp
        var erpApiBase = _settingsDataService.GetErpApiUrl(); 
        var erpApiUri = new Uri(erpApiBase + "/erpapi/SyncMatchedBusinessSuppliers");
        try
        {
            var payload = busItemsRequest;
            var request = new HttpRequestMessage(HttpMethod.Post, erpApiUri)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            var result = await _apiService.MakeAuthenticatedRequestAsync(request);
            var jsonContent = await result.Content.ReadAsStringAsync();
            var erpResponse = JsonSerializer.Deserialize<ErpSynchronizationResponse<ItemFamilyDto>>(jsonContent);
                 
            var stMessage = $"Message: {erpResponse.Message}\n" +
                            $"Added Count: {erpResponse.AddedCount}\n" +
                            $"Failed to Add Count: {erpResponse.FailedToAddCount}\n" +
                            $"Updated Count: {erpResponse.UpdatedCount}\n" +
                            $"Failed to Update Count: {erpResponse.FailedToUpdateCount}\n" +
                            $"Deleted Count: {erpResponse.DeletedCount}\n" +
                            $"Failed to Delete Count: {erpResponse.FailedToDeleteCount}";
            AddLog( stMessage);
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
           LogAndHandleException(ex, "An error occured while sending the suppliers sync request to Erp");
        }
        
    }
    [RelayCommand]
    private async Task MatchSuppliers()
    {
        if (_selectedBusinessSupplier is null || _selectedErpSupplier is null)
        {
            return;
        }

        var matchedSuppliers = new MatchedSuppliersDto()
        {
            BusinessSupplier = new SupplierListDto()
            {
                Id = _selectedBusinessSupplier.Id,
                Code = _selectedBusinessSupplier.Code,
                Name = _selectedBusinessSupplier.Name,
                Afm = _selectedBusinessSupplier.Afm
            },
            ErpSupplier = new SupplierListDto()
            {
                Id = _selectedErpSupplier.Id,
                Code = _selectedErpSupplier.Code,
                Name = _selectedErpSupplier.Name,
                Afm = _selectedErpSupplier.Afm
            }
        };
        MatchedSuppliers.Add(matchedSuppliers);
        Suppliers.Remove(_selectedBusinessSupplier);
        ErpSelectedSuppliers.Remove(_selectedErpSupplier);
        
    }

    private async Task GetSuppliersProcessAsync()
    {
        AddLog("Getting Suppliers from server");
        try
        {
            var apiBaseUrl = _settingsDataService.GetErpApiUrl();
            IsWaitingForResponse = true;
            AddLog($"Contacting Erp to retrieve supplier sync status");
            var url = $"/erpapi/GetErpSyncSuppliers?companycode={_companyCode}";
            var uri = new Uri(apiBaseUrl + url);
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                var result = await _apiService.MakeAuthenticatedRequestAsync(request);
                var jsonContent = await result.Content.ReadAsStringAsync();
                var destinationItems = JsonSerializer.Deserialize<IList<SyncSupplierDto>>(jsonContent);
                var sourceList = new List<SyncSupplierDto>();

                AddLog($"Connected to Erp and retrieved {destinationItems.Count} Erp Sync Suppliers");
                var fetchedSuppliers = await _businessServerDataAccess.GetBusinessServerSupplierListAsync();
                AddLog($"Connected to Business and retrieved {fetchedSuppliers.Count} Business Suppliers");
                foreach (var item in fetchedSuppliers)
                {
                    var sourceItem = new SyncSupplierDto()
                    {
                        BusId = item.Id,
                        BusCode = item.Code,
                        CompanyCode = _companyCode,
                        Name = item.Name,
                        TaxNumber = item.Afm,
                        SourceChecksum = ChecksumHelper.CalculateChecksum(item.Id.ToString()
                            , item.Code, item.Name, item.Afm, _companyCode
                        )
                    };
                    sourceList.Add(sourceItem);
                }

                #region Dictionaries

                var sourceDict = sourceList.ToDictionary(x => x.BusId);

                var destinationDict = destinationItems.ToDictionary(x => x.BusId);

                var toInsert = sourceDict
                    .Where(src => !destinationDict.ContainsKey(src.Key))
                    .Select(pair => pair.Value)
                    .ToList();

                var toUpdate = sourceDict
                    .Where(src =>
                        destinationDict.ContainsKey(src.Key) &&
                        destinationDict[src.Key].SourceChecksum != src.Value.SourceChecksum)
                    .Select(pair => pair.Value)
                    .ToList();

                var toDelete = destinationDict
                    .Where(dest => !sourceDict.ContainsKey(dest.Key))
                    .Select(pair => pair.Value)
                    .ToList();

                #endregion


                var suppliersToInsert = toInsert.Select(x => new SupplierListDto
                {
                    Id = x.BusId,
                    Code = x.BusCode,
                    Name = x.Name,
                    Afm = x.TaxNumber
                }).ToList();
                Suppliers = new ObservableCollection<SupplierListDto>(suppliersToInsert);

                //Suppliers = await _businessServerDataAccess.GetBusinessServerSupplierListAsync();
                SupplierCount = Suppliers.Count;
                AddLog($"Found  {SupplierCount} business suppliers to insert");
            }
           
            catch (HttpRequestException httpEx)
            {
                // Exception for HTTP-specific issues
                var errorMessage =
                    $"Network error: Unable to connect to the server. Please check your connection and try again.";
                AddLog(errorMessage);
                Console.WriteLine($"HttpRequestException: {httpEx.Message}");
                await AppShell.DisplayToastAsync(errorMessage);
            }
            catch (JsonException jsonEx)
            {
                // Exception for JSON deserialization errors
                var errorMessage = "Data parsing error: The server returned data in an unexpected format.";
                AddLog(errorMessage);
                Console.WriteLine($"JsonException: {jsonEx.Message}");
                await AppShell.DisplayToastAsync(errorMessage);
            }
            catch (TaskCanceledException taskEx)
            {
                // Exception for timeout or canceled tasks
                var errorMessage = "Request timeout: The server is taking too long to respond. Please try again later.";
                AddLog(errorMessage);
                Console.WriteLine($"TaskCanceledException: {taskEx.Message}");
                await AppShell.DisplayToastAsync(errorMessage);
            }
            catch (Exception ex)
            {
                // General exception for all unhandled cases
                var errorMessage = $"An unexpected error occurred: {ex.Message}";
                AddLog(errorMessage);
                Console.WriteLine($"General Exception: {ex}");
                await AppShell.DisplayToastAsync(errorMessage);
            }


            url = $"/erpapi/GetErpSuppliers?companycode={_companyCode}";
            uri = new Uri(apiBaseUrl + url);
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                var result = await _apiService.MakeAuthenticatedRequestAsync(request);
                var jsonContent = await result.Content.ReadAsStringAsync();
                var erpResponse = JsonSerializer.Deserialize<IList<SupplierListDto>>(jsonContent);
                ErpSuppliers = new ObservableCollection<SupplierListDto>(erpResponse);
                ErpSelectedSuppliers = new ObservableCollection<SupplierListDto>(erpResponse);
                ErpSupplierCount = ErpSuppliers.Count;
                AddLog($"Connected to Erp and retrieved {ErpSupplierCount} erp suppliers");
            }
            catch (HttpRequestException httpEx)
            {
                // Exception for HTTP-specific issues
                var errorMessage =
                    $"Network error: Unable to connect to the server. Please check your connection and try again.";
                AddLog(errorMessage);
                Console.WriteLine($"HttpRequestException: {httpEx.Message}");
                await AppShell.DisplayToastAsync(errorMessage);
            }
            catch (JsonException jsonEx)
            {
                // Exception for JSON deserialization errors
                var errorMessage = "Data parsing error: The server returned data in an unexpected format.";
                AddLog(errorMessage);
                Console.WriteLine($"JsonException: {jsonEx.Message}");
                await AppShell.DisplayToastAsync(errorMessage);
            }
            catch (TaskCanceledException taskEx)
            {
                // Exception for timeout or canceled tasks
                var errorMessage = "Request timeout: The server is taking too long to respond. Please try again later.";
                AddLog(errorMessage);
                Console.WriteLine($"TaskCanceledException: {taskEx.Message}");
                await AppShell.DisplayToastAsync(errorMessage);
            }
            catch (Exception ex)
            {
                // General exception for all unhandled cases
                var errorMessage = $"An unexpected error occurred: {ex.Message}";
                AddLog(errorMessage);
                Console.WriteLine($"General Exception: {ex}");
                await AppShell.DisplayToastAsync(errorMessage);
            }
        }
        catch (Exception e)
        {
            // _logger.LogError(e, "Error in GetSuppliersProcessAsync");
            AddLog($"Error in GetSuppliersProcessAsync: {e.Message}");
            await AppShell.DisplayToastAsync($"Error in GetSuppliersProcessAsync: {e.Message}");
        }
        finally
        {
            IsWaitingForResponse = false;
        }
    }

    [RelayCommand]
    private void BusinessEntitySelectionChanged(object obj)
    {
        var listView = obj as SfListView;
        _selectedBusinessSupplier = listView?.SelectedItem as SupplierListDto;
    }

    [RelayCommand]
    private void ErpEntitySelectionChanged(object obj)
    {
        var listView = obj as SfListView;
        _selectedErpSupplier = listView?.SelectedItem as SupplierListDto;
    }

    [RelayCommand]
    private void MatchedEntitySelectionChanged(object obj)
    {
        var listView = obj as SfListView;
        var selectedItem = listView?.SelectedItem as MatchedSuppliersDto;
        
    }

    private void LogAndHandleException(Exception ex, string customMessage)
    {
        string errorMessage = ex switch
        {
            TaskCanceledException when ex.InnerException is TimeoutException =>
                $"The request timed out: {ex.Message}",
            HttpRequestException =>
                "Network error: Unable to connect to the server. Please check your connection and try again.",
            JsonException =>
                "Data parsing error: The server returned data in an unexpected format.",
            TaskCanceledException =>
                "Request timeout: The server is taking too long to respond. Please try again later.",
            _ =>
                $"An unexpected error occurred: {ex.Message}"
        };

        AddLog(errorMessage);
        Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");

        // Optionally, display the message using a Toast
        AppShell.DisplayToastAsync(customMessage ?? errorMessage).FireAndForgetSafeAsync();
    }
}