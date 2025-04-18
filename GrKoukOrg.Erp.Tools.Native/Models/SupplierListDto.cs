using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models
{
    public class SupplierListDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("afm")]
        public string Afm { get; set; }
    }

    public class MatchedSuppliersDto
    {
        public SupplierListDto BusinessSupplier { get; set; } = new SupplierListDto();
        public SupplierListDto ErpSupplier { get; set; } = new SupplierListDto();
    }
}