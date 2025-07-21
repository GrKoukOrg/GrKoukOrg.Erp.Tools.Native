using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public class DayCloseResponse
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; } = false;
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
  
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; } = "";
}