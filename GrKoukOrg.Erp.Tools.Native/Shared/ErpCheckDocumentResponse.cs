using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Shared;

public class ErpCheckDocumentResponse
{
    [JsonPropertyName("isSynced")] 
    public bool IsSynced { get; set; } = false;
    [JsonPropertyName("isChanged")] 
    public bool IsChanged { get; set; } = false;
    [JsonPropertyName("canSync")] 
    public bool CanSync { get; set; } = false;
    [JsonPropertyName("message")]
    public string Message { get; set; }=string.Empty;
    [JsonPropertyName("documentId")]
    public int DocumentId { get; set; }
}