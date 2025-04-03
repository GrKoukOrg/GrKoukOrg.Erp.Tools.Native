using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Services;

public interface INavigationParameterService
{
    IList<BuyDocumentDto> BuyDocuments { get; set; } 
}

public class NavigationParameterService : INavigationParameterService
{
    public IList<BuyDocumentDto> BuyDocuments { get; set; } = new List<BuyDocumentDto>();
}