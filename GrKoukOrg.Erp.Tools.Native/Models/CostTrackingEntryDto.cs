using System;
using System.Text.Json.Serialization;

namespace GrKoukOrg.Erp.Tools.Native.Models;

public enum CostDocType
{
    PurchaseInvoice = 1,
    ReturnCreditInvoice = 2,
    DiscountCreditInvoice = 3
}

public class CostTrackingEntryDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("transDate")] public DateTime TransDate { get; set; }
    [JsonPropertyName("itemId")] public int ItemId { get; set; }
    [JsonPropertyName("docType")] public CostDocType DocType { get; set; }
    [JsonPropertyName("qtyDelta")] public decimal QtyDelta { get; set; }
    [JsonPropertyName("valueDelta")] public decimal ValueDelta { get; set; }
    [JsonPropertyName("qtyAfter")] public decimal QtyAfter { get; set; }
    [JsonPropertyName("avgCostAfter")] public decimal AvgCostAfter { get; set; }
    [JsonPropertyName("sourceDocId")] public int? SourceDocId { get; set; }
    [JsonPropertyName("notes")] public string? Notes { get; set; }
}