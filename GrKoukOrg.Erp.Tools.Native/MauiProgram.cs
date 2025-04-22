using CommunityToolkit.Maui;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using ZXing.Net.Maui.Controls;
using ErpCashDiaryListPageModel = GrKoukOrg.Erp.Tools.Native.PageModels.ErpCashDiaryListPageModel;

namespace GrKoukOrg.Erp.Tools.Native
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                
                .UseBarcodeReader()
                .ConfigureSyncfusionToolkit()
                .ConfigureSyncfusionCore()
                .ConfigureMauiHandlers(handlers =>
                {
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif
           
            
           
            builder.Services.AddScoped<LocalItemsRepo>();
           
            builder.Services.AddScoped<LocalBuyDocumentsRepo>();
            builder.Services.AddScoped<LocalBuyDocLinesRepo>();
            builder.Services.AddScoped<LocalSuppliersRepo>();
            builder.Services.AddScoped<LocalSaleDocumentsRepo>();
            builder.Services.AddScoped<LocalSalesDocLinesRepo>();
            builder.Services.AddScoped<LocalCustomerRepo>();
            builder.Services.AddScoped<ISettingsDataService, SettingsMemoryDataService>();
            builder.Services.AddScoped<IBusinessServerDataAccess, BusinessServerHttpDataAccess>();
            builder.Services.AddScoped<ILocalCashDiaryRepo<CashDiaryItemDto>, LocalCashDiaryTestRepo>();
            builder.Services.AddScoped<ApiService>();
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddScoped<ModalErrorHandler>();
            builder.Services.AddScoped<MainPageModel>();
         
            builder.Services.AddScoped<SettingsPageModel>();
            builder.Services.AddSingleton<INavigationParameterService, NavigationParameterService>();
            
            builder.Services.AddTransientWithShellRoute<SettingsPage, SettingsPageModel>("settings");
            builder.Services.AddTransientWithShellRoute<SyncItemsPage,SyncItemsPageModel>("syncitems");
            builder.Services.AddTransientWithShellRoute<ItemDetailsPage, ItemDetailsPageModel>("itemdetails");
            builder.Services.AddTransientWithShellRoute<ErpCashDiaryListPage, ErpCashDiaryListPageModel>("erpcashdiarylist");
            builder.Services.AddTransientWithShellRoute<ItemBuyDocListPage, ItemBuyDocListPageModel>("itembuylist");
            builder.Services.AddTransientWithShellRoute<BusinessBuyDocumentsListPage, BusinessBuyDocumentsListPageModel>("Businessbuydoclist");
            builder.Services.AddTransientWithShellRoute<SyncToErpPage, SyncToErpPageModel>("synctoerp");
            builder.Services.AddTransientWithShellRoute<BusBuyDocListSyncPage, BusBuyDocListSyncPageModel>("busbuydoclistsync");
            builder.Services.AddTransientWithShellRoute<SyncSuppliersPage, SyncSuppliersPageModel>("syncSuppliers");
            
            builder.Services.AddHttpClient("BusinessServerApi", (serviceProvider, client) =>
            {
                var settingsDataService = serviceProvider.GetRequiredService<ISettingsDataService>();
                var apiUrl = settingsDataService.GetBusinessApiUrl();
                client.BaseAddress = new Uri(apiUrl);
            });
            return builder.Build();
        }
    }
}
