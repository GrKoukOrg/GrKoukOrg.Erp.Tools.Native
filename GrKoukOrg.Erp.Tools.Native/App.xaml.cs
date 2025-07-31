using System.Globalization;

namespace GrKoukOrg.Erp.Tools.Native
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzk3NDI1M0AzMzMwMmUzMDJlMzAzYjMzMzAzYk4wcE8vRzFiNUJKQjVWMVRFTHJkLzFLSGoyOTNyenhaQnFEdGc2SVRxUWM9");
            InitializeComponent();
            var culture = new CultureInfo("el-GR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}