using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Shared;

public class ErpSynchronizationResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("addedCount")]
    public int AddedCount { get; set; }
    [JsonPropertyName("FailedToAddCount")]
    public int FailedToAddCount { get; set; }
    [JsonPropertyName("updatedCount")]
    public int UpdatedCount { get; set; }
    [JsonPropertyName("FailedToUpdateCount")]
    public int FailedToUpdateCount { get; set; }
    [JsonPropertyName("deletedCount")]
    public int DeletedCount { get; set; }
    [JsonPropertyName("FailedToDeleteCount")]
    public int FailedToDeleteCount { get; set; }
    [JsonPropertyName("syncSessionId")]
    public Guid SyncSessionId { get; set; }
    [JsonPropertyName("syncSource")]
    public string SyncSource { get; set; }
    [JsonPropertyName("syncItems")]
    public List<T> SyncItems { get; set; }
}
public class ErpCheckDocumentResponse
{
    [JsonPropertyName("isSynced")] 
    public bool IsSynced { get; set; } = false;

    [JsonPropertyName("canSync")] 
    public bool CanSync { get; set; } = false;
    [JsonPropertyName("message")]
    public string Message { get; set; }=string.Empty;
    [JsonPropertyName("documentId")]
    public int DocumentId { get; set; }
}
