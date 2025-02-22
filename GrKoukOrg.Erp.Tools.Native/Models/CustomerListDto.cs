using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class CustomerListDto
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