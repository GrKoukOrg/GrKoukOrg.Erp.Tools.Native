using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using GrKoukOrg.Erp.Tools.Native.Data;

namespace GrKoukOrg.Erp.Tools.Native.Data;

/// <summary>
/// Local repository for inventory cost tracking entries. Follows the design of LocalBuyDocLinesRepo.
/// Tracks three document types:
/// 1. Purchase invoice (increases stock and value)
/// 2. Return credit invoice (decreases stock and value)
/// 3. Discount credit invoice (decreases value only; lowers average cost without changing quantity)
/// </summary>
public class LocalCostTrackingRepo
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    public LocalCostTrackingRepo(ILogger<LocalCostTrackingRepo> logger)
    {
        _logger = logger;
    }

    private async Task Init()
    {
        if (_hasBeenInitialized)
            return;

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        try
        {
            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS CostTrackingEntries (
                Id INTEGER PRIMARY KEY,
                TransDate TEXT NOT NULL,
                ItemId INTEGER NOT NULL,
                DocType INTEGER NOT NULL, -- 1=PurchaseInvoice, 2=ReturnCreditInvoice, 3=DiscountCreditInvoice
                QtyDelta DECIMAL(18,4) NOT NULL,
                ValueDelta DECIMAL(18,4) NOT NULL, -- signed value change (net of tax)
                QtyAfter DECIMAL(18,4) NOT NULL,
                AvgCostAfter DECIMAL(18,6) NOT NULL,
                SourceDocId INTEGER,
                Notes TEXT
            );
            ";
            await createTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating CostTrackingEntries table");
            throw;
        }

        _hasBeenInitialized = true;
    }

    public async Task<List<Models.CostTrackingEntryDto>> ListAsync()
    {
        await Init();
        var list = new List<Models.CostTrackingEntryDto>();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT Id, TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes FROM CostTrackingEntries ORDER BY TransDate, Id";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ReadDto(reader));
        }
        return list;
    }

    public async Task<List<Models.CostTrackingEntryDto>> ListByDateRangeAsync(int itemId, DateTime fromDate, DateTime toDate)
    {
        await Init();
        var list = new List<Models.CostTrackingEntryDto>();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT Id, TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes FROM CostTrackingEntries WHERE ItemId=@itemId AND TransDate>=@fromDate AND TransDate<=@toDate ORDER BY TransDate, Id";
        cmd.Parameters.AddWithValue("@itemId", itemId);
        cmd.Parameters.AddWithValue("@fromDate", fromDate);
        cmd.Parameters.AddWithValue("@toDate", toDate);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ReadDto(reader));
        }
        return list;
    }

    public async Task<bool> EntryExists(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id FROM CostTrackingEntries WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    public async Task<Models.CostTrackingEntryDto?> GetAsync(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT Id, TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes FROM CostTrackingEntries WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadDto(reader);
        }
        return null;
    }

    public async Task<int> AddAsync(Models.CostTrackingEntryDto entry)
    {
        await Init();
        // compute moving average cost and resulting qty after based on previous state
        var (qtyBefore, avgCostBefore) = await GetLastState(entry.ItemId, entry.TransDate);
        var qtyAfter = qtyBefore + entry.QtyDelta;
        decimal avgCostAfter;
        if (qtyAfter == 0)
        {
            // no stock left; keep avg cost at 0 to avoid division by zero
            avgCostAfter = 0m;
        }
        else
        {
            var valueBefore = qtyBefore * avgCostBefore;
            var valueAfter = valueBefore + entry.ValueDelta;
            avgCostAfter = valueAfter / (qtyAfter);
        }

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"INSERT INTO CostTrackingEntries (Id, TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes) VALUES (@id, @transDate, @itemId, @docType, @qtyDelta, @valueDelta, @qtyAfter, @avgCostAfter, @sourceDocId, @notes)";
        cmd.Parameters.AddWithValue("@id", entry.Id);
        cmd.Parameters.AddWithValue("@transDate", entry.TransDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        cmd.Parameters.AddWithValue("@itemId", entry.ItemId);
        cmd.Parameters.AddWithValue("@docType", (int)entry.DocType);
        cmd.Parameters.AddWithValue("@qtyDelta", entry.QtyDelta);
        cmd.Parameters.AddWithValue("@valueDelta", entry.ValueDelta);
        cmd.Parameters.AddWithValue("@qtyAfter", qtyAfter);
        cmd.Parameters.AddWithValue("@avgCostAfter", avgCostAfter);
        cmd.Parameters.AddWithValue("@sourceDocId", entry.SourceDocId.HasValue ? entry.SourceDocId.Value : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@notes", entry.Notes ?? (object)DBNull.Value);
        try
        {
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding cost tracking entry");
            throw;
        }
    }

    public async Task<int> UpdateAsync(Models.CostTrackingEntryDto entry)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"UPDATE CostTrackingEntries SET TransDate=@transDate, ItemId=@itemId, DocType=@docType, QtyDelta=@qtyDelta, ValueDelta=@valueDelta, QtyAfter=@qtyAfter, AvgCostAfter=@avgCostAfter, SourceDocId=@sourceDocId, Notes=@notes WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", entry.Id);
        cmd.Parameters.AddWithValue("@transDate", entry.TransDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        cmd.Parameters.AddWithValue("@itemId", entry.ItemId);
        cmd.Parameters.AddWithValue("@docType", (int)entry.DocType);
        cmd.Parameters.AddWithValue("@qtyDelta", entry.QtyDelta);
        cmd.Parameters.AddWithValue("@valueDelta", entry.ValueDelta);
        cmd.Parameters.AddWithValue("@qtyAfter", entry.QtyAfter);
        cmd.Parameters.AddWithValue("@avgCostAfter", entry.AvgCostAfter);
        cmd.Parameters.AddWithValue("@sourceDocId", entry.SourceDocId.HasValue ? entry.SourceDocId.Value : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@notes", entry.Notes ?? (object)DBNull.Value);
        try
        {
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating cost tracking entry with Id={Id}", entry.Id);
            throw;
        }
    }

    public async Task<int> DeleteAsync(Models.CostTrackingEntryDto entry)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM CostTrackingEntries WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", entry.Id);
        try
        {
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting cost tracking entry with Id={Id}", entry.Id);
            throw;
        }
    }

    public async Task<int> DeleteAllAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM CostTrackingEntries";
        try
        {
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting all cost tracking entries");
            throw;
        }
    }

    public async Task<int> DropTableAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DROP TABLE IF EXISTS CostTrackingEntries";
        try
        {
            return await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error dropping CostTrackingEntries table");
            throw;
        }
    }

    public async Task<decimal> GetCurrentAverageCostAsync(int itemId)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT AvgCostAfter FROM CostTrackingEntries WHERE ItemId=@itemId ORDER BY TransDate DESC, Id DESC LIMIT 1";
        cmd.Parameters.AddWithValue("@itemId", itemId);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader.GetDecimal(0);
        }
        return 0m;
    }

    public async Task<int> AddPurchaseInvoiceAsync(int itemId, DateTime transDate, decimal quantity, decimal unitPrice, int? sourceDocId = null, string? notes = null)
    {
        var valueDelta = quantity * unitPrice; // net value
        var entry = new Models.CostTrackingEntryDto
        {
            Id = 0, // caller can supply explicit id if needed; 0 by default
            TransDate = transDate,
            ItemId = itemId,
            DocType = Models.CostDocType.PurchaseInvoice,
            QtyDelta = quantity,
            ValueDelta = valueDelta,
            SourceDocId = sourceDocId,
            Notes = notes
        };
        return await AddAsync(entry);
    }

    public async Task<int> AddReturnCreditInvoiceAsync(int itemId, DateTime transDate, decimal quantity, decimal unitPrice, int? sourceDocId = null, string? notes = null)
    {
        var valueDelta = -quantity * unitPrice; // reduce value
        var entry = new Models.CostTrackingEntryDto
        {
            Id = 0,
            TransDate = transDate,
            ItemId = itemId,
            DocType = Models.CostDocType.ReturnCreditInvoice,
            QtyDelta = -quantity, // reduce stock
            ValueDelta = valueDelta,
            SourceDocId = sourceDocId,
            Notes = notes
        };
        return await AddAsync(entry);
    }

    public async Task<int> AddDiscountCreditInvoiceAsync(int itemId, DateTime transDate, decimal amount, int? sourceDocId = null, string? notes = null)
    {
        // amount returned by supplier decreases inventory value; stock qty unchanged
        var entry = new Models.CostTrackingEntryDto
        {
            Id = 0,
            TransDate = transDate,
            ItemId = itemId,
            DocType = Models.CostDocType.DiscountCreditInvoice,
            QtyDelta = 0m,
            ValueDelta = -Math.Abs(amount),
            SourceDocId = sourceDocId,
            Notes = notes
        };
        return await AddAsync(entry);
    }

    private static Models.CostTrackingEntryDto ReadDto(SqliteDataReader reader)
    {
        return new Models.CostTrackingEntryDto
        {
            Id = reader.GetInt32(0),
            TransDate = reader.GetDateTime(1),
            ItemId = reader.GetInt32(2),
            DocType = (Models.CostDocType)reader.GetInt32(3),
            QtyDelta = reader.GetDecimal(4),
            ValueDelta = reader.GetDecimal(5),
            QtyAfter = reader.GetDecimal(6),
            AvgCostAfter = reader.GetDecimal(7),
            SourceDocId = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
            Notes = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }

    private async Task<(decimal qtyBefore, decimal avgCostBefore)> GetLastState(int itemId, DateTime transDate)
    {
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        // Find latest entry up to the current transaction date
        cmd.CommandText = @"SELECT QtyAfter, AvgCostAfter FROM CostTrackingEntries WHERE ItemId=@itemId AND TransDate<=@transDate ORDER BY TransDate DESC, Id DESC LIMIT 1";
        cmd.Parameters.AddWithValue("@itemId", itemId);
        cmd.Parameters.AddWithValue("@transDate", transDate);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var qty = reader.GetDecimal(0);
            var avg = reader.GetDecimal(1);
            return (qty, avg);
        }
        return (0m, 0m);
    }
}
