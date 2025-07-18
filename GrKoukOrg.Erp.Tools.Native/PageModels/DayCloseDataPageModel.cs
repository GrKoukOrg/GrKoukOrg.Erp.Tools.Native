using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.PageModels;

public partial class DayCloseDataPageModel:ObservableObject
{
    private readonly ILogger<BusBuyDocListSyncPageModel> _logger;
    private readonly ApiService _apiService;
    private readonly ISettingsDataService _settingsDataService;
    private readonly INavigationParameterService _navParameterService;
    
    
    [ObservableProperty] private bool _isWaitingForResponse = false;
    [ObservableProperty] private DateTime _closingDate = DateTime.Today;
    [ObservableProperty] private decimal _totalCash = 0;
    [ObservableProperty] private decimal _totalCards = 0;   
    [ObservableProperty] private decimal _totalStar = 0;
    [ObservableProperty] private int _zNumber = 0;
    private string _companyCode;
    private int _lastZNumber = 0;
    
    public DayCloseDataPageModel(ILogger<BusBuyDocListSyncPageModel> logger, ApiService apiService,
        ISettingsDataService settingsDataService, INavigationParameterService navParameterService)
    {
        _logger = logger;
        _apiService = apiService;
        _settingsDataService = settingsDataService;
        _navParameterService = navParameterService;
    }
    [RelayCommand]
    private async Task Appearing()
    {
        IsWaitingForResponse = false;
        _companyCode = _settingsDataService.GetBusinessCompanyCode();
        _lastZNumber=_settingsDataService.GetLastZNumber();
    }
}