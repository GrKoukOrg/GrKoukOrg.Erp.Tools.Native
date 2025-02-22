using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class SaleDocListDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("transDate")]
    public DateTime TransDate { get; set; }
    [JsonPropertyName("saleDocDefId")]
    public int SaleDocDefId { get; set; }
    [JsonPropertyName("saleDocDefName")]
    public string? SaleDocDefName { get; set; }
    [JsonPropertyName("customerId")]

    public int CustomerId { get; set; }
    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }
    [JsonPropertyName("refNumber")]

    public string? RefNumber { get; set; }
    [JsonPropertyName("netAmount")]
    public decimal NetAmount { get; set; }
    [JsonPropertyName("vatAmount")]
    public decimal VatAmount { get; set; }
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

}