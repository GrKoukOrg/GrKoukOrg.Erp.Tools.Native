using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Services;

public interface IStartupChecker
{
    Task<StartupCheckResult> PerformAllChecksAsync();
}