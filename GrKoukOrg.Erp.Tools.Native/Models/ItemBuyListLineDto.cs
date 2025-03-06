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
    public string? RefNumber { get; set; }
    
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
    
    [JsonPropertyName("unitDiscountAmount")]
    public decimal UnitDiscountAmount { get; set; }
    
    [JsonPropertyName("unitNetAmount")]
    public decimal UnitNetAmount { get; set; }
    
    [JsonPropertyName("unitVatAmount")]
    public decimal UnitVatAmount { get; set; }
    
    [JsonPropertyName("unitTotalAmount")]
    public decimal UnitTotalAmount { get; set; }
}