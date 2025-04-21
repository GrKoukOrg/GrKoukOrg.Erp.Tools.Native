using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Services;

public interface INavigationParameterService
{
    IList<BusinessBuyDocumentDto> BuyDocuments { get; set; } 
}

public class NavigationParameterService : INavigationParameterService
{
    public IList<BusinessBuyDocumentDto> BuyDocuments { get; set; } = new List<BusinessBuyDocumentDto>();
}