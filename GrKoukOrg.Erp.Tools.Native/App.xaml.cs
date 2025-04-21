using System.Globalization;

namespace GrKoukOrg.Erp.Tools.Native
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzgyNDk4M0AzMjM5MmUzMDJlMzAzYjMyMzkzYmJTMlEvMHNQUzVma2FMMnZrakpidFdjcFpVRlBJWVhpU2xxS2wrRUJ4Z0k9");
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