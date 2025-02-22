using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class SaleDocLineListDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("transDate")]
    public DateTime TransDate { get; set; }
    [JsonPropertyName("saleDocId")]
    public int SaleDocId { get; set; }
    [JsonPropertyName("itemId")]
    public int ItemId { get; set; }
    [JsonPropertyName("itemName")]
    public string? ItemName { get; set; }
    [JsonPropertyName("itemCode")]
    public string? ItemCode { get; set; }
    //public string? UnitOfMeasureCode { get; set; }
    [JsonPropertyName("unitOfMeasureName")]
    public string? UnitOfMeasureName { get; set; }
    [JsonPropertyName("unitFpaPerc")]
    public double UnitFpaPerc { get; set; }
    [JsonPropertyName("unitQty")]
    public decimal UnitQty { get; set; }
    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
    [JsonPropertyName("unitDiscountRate")]
    public double UnitDiscountRate { get; set; }
    [JsonPropertyName("unitDiscountAmount")]
    public decimal UnitDiscountAmount { get; set; }
    [JsonPropertyName("unitNetAmount")]
    public decimal UnitNetAmount { get; set; }
    [JsonPropertyName("unitVatAmount")]
    public decimal UnitVatAmount { get; set; }
    [JsonPropertyName("unitTotalAmount")]
    public decimal UnitTotalAmount { get; set; }

}