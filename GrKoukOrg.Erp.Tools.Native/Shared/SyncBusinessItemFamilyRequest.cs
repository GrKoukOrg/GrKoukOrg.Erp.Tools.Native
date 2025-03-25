using System.Text.Json.Serialization;
using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Shared;

/// <summary>
/// This definition is copied from the Erp
/// Make sure to update any changes made there
/// </summary>
public class SyncBusinessItemFamilyRequest
{
    private IList<ItemFamilyDto> _items;
    
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    
    [JsonPropertyName("items")]
    public IList<ItemFamilyDto> Items
    {
        get { return _items ??= new List<ItemFamilyDto>() ; }
        set => _items = value;
    }
}
public class BusinessApiResponse<T>
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("items")]
    public T Items { get; set; }
}
