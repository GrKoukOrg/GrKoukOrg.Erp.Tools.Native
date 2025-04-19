using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Shared;

public class SyncBusinessBuyDocumentRequest
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("transDate")]
    public DateTime TransDate { get; set; }
    [JsonPropertyName("supplierId")]
    public int SupplierId { get; set; }
    [JsonPropertyName("refNumber")]
    public int RefNumber { get; set; }
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }
    [JsonPropertyName("payedAmount")]
    public decimal PayedAmount { get; set; } 
   
}