using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class ItemBuyListLineDto
{
    [JsonPropertyName("transDate")]
    public DateTime TransDate { get; set; }

    [JsonPropertyName("buyDocDefName")]
    public string? BuyDocDefName { get; set; }
    
    [JsonPropertyName("supplierId")]
    public int SupplierId { get; set; }
    
    [JsonPropertyName("supplierName")]
    public string? SupplierName { get; set; }
    
    [JsonPropertyName("refNumber")]
    public int RefNumber { get; set; }
    
    [JsonPropertyName("itemId")]
    public int ItemId { get; set; }
    
    [JsonPropertyName("itemName")]
    public string? ItemName { get; set; }
    
    [JsonPropertyName("itemCode")]
    public string? ItemCode { get; set; }
    
    [JsonPropertyName("unitOfMeasureName")]
    public string? UnitOfMeasureName { get; set; }
    
    [JsonPropertyName("unitQty")]
    public decimal UnitQty { get; set; }
   
    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
    
    [JsonPropertyName("unitDiscountRate")]
    public double UnitDiscountRate { get; set; }
    
    [JsonPropertyName("lineDiscountAmount")]
    public decimal LineDiscountAmount { get; set; }
    
    [JsonPropertyName("lineNetAmount")]
    public decimal LineNetAmount { get; set; }
    
    [JsonPropertyName("lineVatAmount")]
    public decimal LineVatAmount { get; set; }
    
    [JsonPropertyName("lineTotalAmount")]
    public decimal LineTotalAmount { get; set; }
    
    [JsonPropertyName("unitFinalPriceBrut")]
    public decimal UnitFinalPriceBrut => UnitQty==0 ? 0: LineTotalAmount/UnitQty;
    [JsonPropertyName("unitFinalPriceNet")]
    public decimal UnitFinalPriceNet => UnitQty == 0 ? 0 : (LineNetAmount - LineDiscountAmount) / UnitQty;
}