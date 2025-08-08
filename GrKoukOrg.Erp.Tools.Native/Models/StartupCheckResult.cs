namespace GrKoukOrg.Erp.Tools.Native.Models;

public class StartupCheckResult
{
    public bool NetworkConnected { get; set; }
    public bool InternetAccessible { get; set; }
    public bool ApiConnected { get; set; }
    public bool TokenRefreshSuccess { get; set; }
    public string ErrorMessage { get; set; }
}