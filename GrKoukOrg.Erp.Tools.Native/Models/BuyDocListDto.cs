using System;

namespace GrKoukOrg.Erp.Tools.Native.Models
{
    public class BuyDocListDto
    {
        public int Id { get; set; }
        public DateTime TransDate { get; set; }
        public int BuyDocDefId { get; set; }
        public string? BuyDocDefName { get; set; }

        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }

        public string? RefNumber { get; set; }
        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }

    }
}