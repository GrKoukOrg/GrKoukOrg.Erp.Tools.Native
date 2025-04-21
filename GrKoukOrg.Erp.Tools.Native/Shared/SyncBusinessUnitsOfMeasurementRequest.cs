using System.Text.Json.Serialization;
using GrKoukOrg.Erp.Tools.Native.Models;

namespace GrKoukOrg.Erp.Tools.Native.Shared;

public class SyncBusinessUnitsOfMeasurementRequest
{
    private IList<UnitOfMeasurementDto> _items;
    
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    
    [JsonPropertyName("items")]
    public IList<UnitOfMeasurementDto> Items
    {
        get { return _items ??= new List<UnitOfMeasurementDto>() ; }
        set => _items = value;
    }
}