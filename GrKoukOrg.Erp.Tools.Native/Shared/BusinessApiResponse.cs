using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Shared;

public class BusinessApiResponse<T>
{
    [JsonPropertyName("companyCode")]
    public string CompanyCode { get; set; }
    [JsonPropertyName("items")]
    public T Items { get; set; }
}