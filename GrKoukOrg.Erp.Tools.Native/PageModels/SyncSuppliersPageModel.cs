using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using GrKoukOrg.Erp.Tools.Native.Shared;
using Syncfusion.Maui.ListView;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class SyncSuppliersPageModel:ObservableObject
{
    private readonly ApiService _apiService;
    private readonly ISettingsDataService _settingsDataService;
    private readonly IBusinessServerDataAccess _businessServerDataAccess;
    private string _companyCode;
    [ObservableProperty] private bool _isWaitingForResponse = false;
    [ObservableProperty]
    private ObservableCollection<SupplierListDto> _suppliers = new ObservableCollection<SupplierListDto>(new List<SupplierListDto>());
    [ObservableProperty]
    private ObservableCollection<SupplierListDto> _erpSuppliers = new ObservableCollection<SupplierListDto>(new List<SupplierListDto>());
    [ObservableProperty]
    private ObservableCollection<SupplierListDto> _erpSelectedSuppliers = new ObservableCollection<SupplierListDto>(new List<SupplierListDto>());

    [ObservableProperty] private int _supplierCount = 0;
    [ObservableProperty] private int _erpSupplierCount = 0;
    
    public ObservableCollection<LogEntry> LogEntries { get; } = new();
    [ObservableProperty] private int _lastLogEntryIndex;
    
    public SyncSuppliersPageModel(ApiService apiService, ISettingsDataService settingsDataService, IBusinessServerDataAccess businessServerDataAccess)
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
    
    
    private async Task GetSuppliersProcessAsync()
    {
        AddLog("Getting Suppliers from server");
        try
        {
            IsWaitingForResponse = true;
            var fetchedSuppliers = await _businessServerDataAccess.GetBusinessServerSupplierListAsync();
            Suppliers = new ObservableCollection<SupplierListDto>(fetchedSuppliers);

            //Suppliers = await _businessServerDataAccess.GetBusinessServerSupplierListAsync();
            SupplierCount = Suppliers.Count;
            AddLog($"Connected and retrieved {SupplierCount} suppliers");
            var apiBaseUrl = _settingsDataService.GetErpApiUrl();
            var url = $"/erpapi/GetErpSuppliers?companycode={_companyCode}";
            var uri = new Uri(apiBaseUrl + url);
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
            catch (Exception ex)
            {
                AddLog($"Error: {ex.Message}");
                Console.WriteLine(ex);
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
        var selectedItem = listView?.SelectedItem as SupplierListDto;
        //DisplayAlert("Message", (listView.SelectedItem as Microsoft.Maui.ApplicationModel.Communication.Contacts).ContactName + " is selected", "OK");
    }
}