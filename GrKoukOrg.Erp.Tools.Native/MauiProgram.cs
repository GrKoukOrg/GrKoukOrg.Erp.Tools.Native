using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using ZXing.Net.Maui.Controls;

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
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif
           
            
           
            builder.Services.AddSingleton<LocalItemsRepo>();
            builder.Services.AddSingleton<LocalBuyDocumentsRepo>();
            builder.Services.AddSingleton<ISettingsDataService, SettingsMemoryDataService>();
            builder.Services.AddSingleton<IBusinessServerDataAccess, BusinessServerHttpDataAccess>();

            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<SeedDataService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<MainPageModel>();
         
            builder.Services.AddSingleton<SettingsPageModel>();
            
           
            builder.Services.AddTransientWithShellRoute<SettingsPage, SettingsPageModel>("settings");
            builder.Services.AddTransientWithShellRoute<SyncItemsPage,SyncItemsPageModel>("syncitems");
            builder.Services.AddTransientWithShellRoute<ItemDetailsPage, ItemDetailsPageModel>("itemdetails");
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
