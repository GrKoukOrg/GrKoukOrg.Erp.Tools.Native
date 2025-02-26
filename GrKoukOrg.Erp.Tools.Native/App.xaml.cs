using System.Globalization;

namespace GrKoukOrg.Erp.Tools.Native
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzY2OTAwM0AzMjM4MmUzMDJlMzBpV0RjOFR3VzhPL0RFSU8xT1lIVjJFVktHR0k1Y3M1ZUYxOVNyNTZMTldFPQ==");
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