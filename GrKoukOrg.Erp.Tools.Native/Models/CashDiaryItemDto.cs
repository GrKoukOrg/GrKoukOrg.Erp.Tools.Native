using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class CashDiaryItemDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("transDate")]
    public DateTime TransDate { get; set; }
    [JsonPropertyName("transId")]
    public int TransId { get; set; }
    [JsonPropertyName("transName")]
    public string TransName { get; set; }
    [JsonPropertyName("etiologia")]
    public string? Etiologia { get; set; }
    [JsonPropertyName("netAmount")]
    public decimal NetAmount { get; set; }
    [JsonPropertyName("vatAmount")]
    public decimal VatAmount { get; set; }
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }
}