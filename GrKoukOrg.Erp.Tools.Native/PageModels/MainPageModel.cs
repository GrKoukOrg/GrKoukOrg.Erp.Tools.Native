using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Maui.Storage;

namespace GrKoukOrg.Erp.Tools.Native.PageModels
{
    public partial class MainPageModel : ObservableObject
    {
        private bool _isNavigatedTo;
        private bool _dataLoaded;

        private readonly ModalErrorHandler _errorHandler;
        private readonly ApiService _apiService;
        private readonly SeedDataService _seedDataService;

        [ObservableProperty] bool _isBusy;

        [ObservableProperty] bool _isRefreshing;

        [ObservableProperty] private string _today = DateTime.Now.ToString("dddd, MMM d");
        [ObservableProperty] private string _lastSynced = string.Empty;

        [ObservableProperty] private string _userName = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _statusMessage = string.Empty;
        public MainPageModel(SeedDataService seedDataService, ModalErrorHandler errorHandler, ApiService apiService)
        {
            _errorHandler = errorHandler;
            _apiService = apiService;
            _seedDataService = seedDataService;
        }

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;
                LastSynced=Preferences.Default.Get("last_synced", string.Empty);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task InitData(SeedDataService seedDataService)
        {
            bool isSeeded = Preferences.Default.ContainsKey("is_seeded");

            if (!isSeeded)
            {
                //await seedDataService.LoadSeedDataAsync();
            }

            Preferences.Default.Set("is_seeded", true);
            await Refresh();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                // await LoadData();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void NavigatedTo() =>
            _isNavigatedTo = true;

        [RelayCommand]
        private void NavigatedFrom() =>
            _isNavigatedTo = false;

        [RelayCommand]
        private async Task Appearing()
        {
            if (!_dataLoaded)
            {
                await InitData(_seedDataService);
                _dataLoaded = true;
                await Refresh();
            }
            // This means we are being navigated to
            else if (!_isNavigatedTo)
            {
                await Refresh();
            }
        }
        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter both username and password.";
                return;
            }

            IsBusy = true;
            StatusMessage = string.Empty;

            try
            {
                var tokens = await _apiService.LoginAsync(UserName, Password);
                if (tokens != null)
                {
                    await SecureStorage.SetAsync("AccessToken", tokens.AccessToken);
                    await SecureStorage.SetAsync("RefreshToken", tokens.RefreshToken);

                    StatusMessage = "Login successful!";
                }
                else
                {
                    StatusMessage = "Invalid login credentials.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}