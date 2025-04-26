using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Views;
using System.Threading.Channels;
using Font = Microsoft.Maui.Font;
using SelectionChangedEventArgs = Syncfusion.Maui.Buttons.SelectionChangedEventArgs;

namespace GrKoukOrg.Erp.Tools.Native
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            var currentTheme = Application.Current!.UserAppTheme;
            ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;
        }
        public static async Task DisplaySnackbarAsync(string message)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF3300"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(0),
                Font = Font.SystemFontOfSize(18),
                ActionButtonFont = Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        public static async Task DisplayToastAsync(string message)
        {
            // Toast is currently not working in MCT on Windows
            // I commented it out for now.
            // if (OperatingSystem.IsWindows())
            // {
            //     var currentPage = Application.Current?.MainPage;
            //
            //     if (currentPage != null)
            //     {
            //         await currentPage.DisplayAlert("Alert", message, "Ok");
            //     }
            //     else
            //     {
            //         Console.WriteLine("No active page found to display the alert.");
            //     }
            //     return;
            // }
                 

            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            Application.Current!.UserAppTheme = selectionChangedEventArgs.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }
    }
}
