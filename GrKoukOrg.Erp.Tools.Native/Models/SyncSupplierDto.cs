using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class SyncSupplierDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("busId")]
    public int BusId { get; set; }
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("busCode")]
    public string BusCode { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("erpId")]
    public int ErpId { get; set; }
    [JsonPropertyName("taxNumber")]
    public string TaxNumber { get; set; }
    [JsonPropertyName("sourceChecksum")]
    public string SourceChecksum { get; set; }
}