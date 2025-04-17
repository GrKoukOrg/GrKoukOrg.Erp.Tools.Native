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

public class BusinessApiResponse<T>
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("items")]
    public T Items { get; set; }
}

public class ErpSynchronizationResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("addedCount")]
    public int AddedCount { get; set; }
    [JsonPropertyName("FailedToAddCount")]
    public int FailedToAddCount { get; set; }
    [JsonPropertyName("updatedCount")]
    public int UpdatedCount { get; set; }
    [JsonPropertyName("FailedToUpdateCount")]
    public int FailedToUpdateCount { get; set; }
    [JsonPropertyName("deletedCount")]
    public int DeletedCount { get; set; }
    [JsonPropertyName("FailedToDeleteCount")]
    public int FailedToDeleteCount { get; set; }
    [JsonPropertyName("syncSessionId")]
    public Guid SyncSessionId { get; set; }
    [JsonPropertyName("syncSource")]
    public string SyncSource { get; set; }
    [JsonPropertyName("syncItems")]
    public List<T> SyncItems { get; set; }
}