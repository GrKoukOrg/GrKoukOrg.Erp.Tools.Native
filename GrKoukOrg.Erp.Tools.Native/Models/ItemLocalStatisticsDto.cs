using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class ItemLocalStatisticsDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
}