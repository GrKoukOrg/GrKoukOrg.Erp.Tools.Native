using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class ItemPurchaseAggregatesDto
{
    [JsonPropertyName("itemId")] public int ItemId { get; set; }
    [JsonPropertyName("totalQuantityPurchased")] public decimal TotalQuantityPurchased { get; set; }
    [JsonPropertyName("totalPurchaseCost")] public decimal TotalPurchaseCost { get; set; }
    [JsonPropertyName("totalDiscountCost")] public decimal TotalDiscountCost { get; set; }
}

public class ItemSalesAggregatesDto
{
    [JsonPropertyName("itemId")] public int ItemId { get; set; }
    [JsonPropertyName("totalQuantitySold")] public decimal TotalQuantitySold { get; set; }
    [JsonPropertyName("totalSaleIncome")] public decimal TotalSaleIncome { get; set; }
}