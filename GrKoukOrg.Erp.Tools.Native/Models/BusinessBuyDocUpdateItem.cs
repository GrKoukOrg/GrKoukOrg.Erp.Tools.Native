using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class BusinessBuyDocUpdateItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("transDate")]
    public DateTime TransDate { get; set; }
    [JsonPropertyName("buyDocDefId")]
    public int BuyDocDefId { get; set; }
    [JsonPropertyName("buyDocDefName")]
    public string? BuyDocDefName { get; set; }
    [JsonPropertyName("supplierId")]

    public int SupplierId { get; set; }
    [JsonPropertyName("supplierName")]
    public string? SupplierName { get; set; }
    [JsonPropertyName("refNumber")]

    public int RefNumber { get; set; }
    [JsonPropertyName("netAmount")]
    public decimal NetAmount { get; set; }
    [JsonPropertyName("vatAmount")]
    public decimal VatAmount { get; set; }
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }
    [JsonPropertyName("payedAmount")]
    public decimal PayedAmount { get; set; }
    [JsonPropertyName("buyDocLines")]
    public List<BuyDocLineDto> BuyDocLines { get; set; } = new List<BuyDocLineDto>();
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
    [JsonPropertyName("isSynced")]
    public bool IsSynced { get; set; }
    [JsonPropertyName("canSync")]
    public bool CanSync { get; set; }
    [JsonPropertyName("isSendingToErp")]
    public bool IsSendingToErp { get; set; }
}