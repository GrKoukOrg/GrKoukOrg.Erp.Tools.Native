using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Shared;

/// <summary>
/// Request for syncing entity of type T
/// </summary>
/// <typeparam name="T"></typeparam>
public class SyncBusinessEntityRequest<T>
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    
    private IList<T> _items;
    [JsonPropertyName("items")]
    public IList<T> Items
    {
        get { return _items ??= new List<T>() ; }
        set => _items = value;
    } 
}