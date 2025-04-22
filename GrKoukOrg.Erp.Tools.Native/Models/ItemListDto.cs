using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class ItemListDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("measureUnitId")]
    public int? MeasureUnitId { get; set; }
    [JsonPropertyName("measureUnitName")]
    public string? MeasureUnitName { get; set; }
    [JsonPropertyName("category")]
    public int? CategoryId { get; set; }
    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }
    [JsonPropertyName("fpaCategoryId")]
    public int? FpaCategoryId { get; set; }
    [JsonPropertyName("fpaCategoryName")]
    public string? FpaCategoryName { get; set; }
    [JsonPropertyName("barcodes")]
    public string? Barcodes { get; set; }
    [JsonPropertyName("apothema")]
    public decimal? Apothema{ get; set; }
    [JsonPropertyName("timiAgoras")]
    public decimal? TimiAgoras { get; set; }
    [JsonPropertyName("timiAgorasFpa")]
    public decimal? TimiAgorasFpa { get; set; }
    [JsonPropertyName("timiPolisisLian")]
    public decimal? TimiPolisisLian { get; set; }
    [JsonPropertyName("timiPolisisLianFpa")]
    public decimal? TimiPolisisLianFpa { get; set; }
    
}
public class ItemStatisticsDto
{
    [JsonPropertyName("itemId")]
    public int ItemId { get; set; }
    [JsonPropertyName("meanPrice")]
    public decimal MeanPrice { get; set; }
    [JsonPropertyName("totalQuantityInWarehouse")]
    public decimal TotalQuantityInWarehouse { get; set; }
    [JsonPropertyName("meanSalesPrice")]
    public decimal MeanSalesPrice { get; set; }
    [JsonPropertyName("totalQuantitySold")]
    public decimal TotalQuantitySold { get; set; } 
    [JsonPropertyName("markUpPercentage")] 
    public decimal MarkUpPercentage { get; set; }
    [JsonPropertyName("markUpAmountNet")] 
    public decimal MarkUpAmountNet { get; set; }
    [JsonPropertyName("markUpAmountBrut")] 
    public decimal MarkUpAmountBrut { get; set; }
}

/// <summary>
/// This must be compatible with the equivalent
/// definition in Erp
/// </summary>
public class ItemFamilyDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    // [JsonPropertyName("code")]
    // public string Code { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

/// <summary>
/// This must be compatible with the equivalent
/// definition in Erp
/// </summary>
public class UnitOfMeasurementDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    // [JsonPropertyName("code")]
    // public string Code { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}