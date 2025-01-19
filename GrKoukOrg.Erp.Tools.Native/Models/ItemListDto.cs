namespace GrKoukOrg.Erp.Tools.Native.Models;

public class ItemListDto
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int? MeasureUnitId { get; set; }
    public string? MeasureUnitName { get; set; }
    public int? FpaCategoryId { get; set; }
    public string? FpaCategoryName { get; set; }
    public string? Barcodes { get; set; }
    public decimal? Apothema{ get; set; }
    public decimal? TimiAgoras { get; set; }
    public decimal? TimiAgorasFpa { get; set; }
    public decimal? TimiPolisisLian { get; set; }
    public decimal? TimiPolisisLianFpa { get; set; }
    
}