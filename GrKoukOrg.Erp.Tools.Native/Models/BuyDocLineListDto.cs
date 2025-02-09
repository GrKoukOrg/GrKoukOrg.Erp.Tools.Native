namespace GrKoukOrg.Erp.Tools.Native.Models;

public class BuyDocLineListDto
{
    public int Id { get; set; }
    public DateTime TransDate { get; set; }
    public int BuyDocId { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    //public string? UnitOfMeasureCode { get; set; }
    public string? UnitOfMeasureName { get; set; }
    public double UnitFpaPerc { get; set; }
    public decimal UnitQty { get; set; }
    public decimal UnitPrice { get; set; }
    public double UnitDiscountRate { get; set; }
    public decimal UnitDiscountAmount { get; set; }
    public decimal UnitNetAmount { get; set; }
    public decimal UnitVatAmount { get; set; }
    public decimal UnitTotalAmount { get; set; }

}