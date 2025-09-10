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

    public async Task<int> UpsertAndRecalculateAsync(Models.CostTrackingEntryDto entry)
    {
        await Init();
        // Find existing by composite natural key
        var existing = await GetBySourceAsync(entry.SourceType, entry.SourceLineId);
        DateTime fromDate;
        int itemId;

        if (existing == null)
        {
            // Insert using existing AddAsync to populate initial QtyAfter/AvgCostAfter
            await AddAsync(entry);
            fromDate = entry.TransDate;
            itemId = entry.ItemId;
        }
        else
        {
            // Update the row first with provided deltas and metadata; keep current QtyAfter/AvgCostAfter (to be recalculated)
            var temp = new Models.CostTrackingEntryDto
            {
                Id = existing.Id, // entry Id is the surrogate PK
                TransDate = entry.TransDate,
                ItemId = entry.ItemId,
                DocType = entry.DocType,
                QtyDelta = entry.QtyDelta,
                ValueDelta = entry.ValueDelta,
                QtyAfter = existing.QtyAfter,
                AvgCostAfter = existing.AvgCostAfter,
                SourceDocId = entry.SourceDocId,
                Notes = entry.Notes,
                SourceType = existing.SourceType,
                SourceLineId = existing.SourceLineId
            };
            await UpdateAsync(temp);
            fromDate = entry.TransDate <= existing.TransDate ? entry.TransDate : existing.TransDate;
            itemId = entry.ItemId;
        }

        // Recalculate downstream running averages and quantities for this item
        await RecalculateFromAsync(itemId, fromDate);
        return 1;
    }

    public async Task RecalculateFromAsync(int itemId, DateTime fromDate)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        // Get the state immediately before fromDate
        decimal qtyBefore = 0m;
        decimal avgCostBefore = 0m;
        {
            var cmdState = connection.CreateCommand();
            cmdState.CommandText = @"SELECT QtyAfter, AvgCostAfter FROM CostTrackingEntries WHERE ItemId=@itemId AND TransDate<@fromDate ORDER BY TransDate DESC, Id DESC LIMIT 1";
            cmdState.Parameters.AddWithValue("@itemId", itemId);
            cmdState.Parameters.AddWithValue("@fromDate", fromDate);
            await using var readerState = await cmdState.ExecuteReaderAsync();
            if (await readerState.ReadAsync())
            {
                qtyBefore = readerState.GetDecimal(0);
                avgCostBefore = readerState.GetDecimal(1);
            }
        }

        // Fetch entries to recalc
        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"SELECT Id, TransDate, QtyDelta, ValueDelta FROM CostTrackingEntries WHERE ItemId=@itemId AND TransDate>=@fromDate ORDER BY TransDate, Id";
        selectCmd.Parameters.AddWithValue("@itemId", itemId);
        selectCmd.Parameters.AddWithValue("@fromDate", fromDate);

        var entries = new List<(int Id, DateTime TransDate, decimal QtyDelta, decimal ValueDelta)>();
        await using (var reader = await selectCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                entries.Add((reader.GetInt32(0), reader.GetDateTime(1), reader.GetDecimal(2), reader.GetDecimal(3)));
            }
        }

        decimal runningQty = qtyBefore;
        decimal runningAvg = avgCostBefore;
        foreach (var e in entries)
        {
            var valueBefore = runningQty * runningAvg;
            var qtyAfter = runningQty + e.QtyDelta;
            decimal avgAfter;
            if (qtyAfter == 0m)
            {
                avgAfter = 0m;
            }
            else
            {
                var valueAfter = valueBefore + e.ValueDelta;
                avgAfter = valueAfter / qtyAfter;
            }

            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"UPDATE CostTrackingEntries SET QtyAfter=@qtyAfter, AvgCostAfter=@avgAfter WHERE Id=@id";
            updateCmd.Parameters.AddWithValue("@qtyAfter", qtyAfter);
            updateCmd.Parameters.AddWithValue("@avgAfter", avgAfter);
            updateCmd.Parameters.AddWithValue("@id", e.Id);
            await updateCmd.ExecuteNonQueryAsync();

            runningQty = qtyAfter;
            runningAvg = avgAfter;
        }
    }

    private async Task Init()
    {
        if (_hasBeenInitialized)
            return;

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        try
        {
            // Detect existing schema
            var pragmaCmd = connection.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA table_info('CostTrackingEntries')";
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var reader = await pragmaCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    existingColumns.Add(reader.GetString(1)); // column name
                }
            }

            var needsMigration = existingColumns.Count > 0 && (!existingColumns.Contains("SourceType") || !existingColumns.Contains("SourceLineId"));
            var tableMissing = existingColumns.Count == 0;

            if (needsMigration)
            {
                // Rename old table
                var renameCmd = connection.CreateCommand();
                renameCmd.CommandText = "ALTER TABLE CostTrackingEntries RENAME TO CostTrackingEntries_old";
                await renameCmd.ExecuteNonQueryAsync();

                // Create new table with the updated schema
                var createNewCmd = connection.CreateCommand();
                createNewCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS CostTrackingEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TransDate TEXT NOT NULL,
                    ItemId INTEGER NOT NULL,
                    DocType INTEGER NOT NULL,
                    QtyDelta DECIMAL(18,4) NOT NULL,
                    ValueDelta DECIMAL(18,4) NOT NULL,
                    QtyAfter DECIMAL(18,4) NOT NULL,
                    AvgCostAfter DECIMAL(18,6) NOT NULL,
                    SourceDocId INTEGER,
                    Notes TEXT,
                    SourceType INTEGER NOT NULL,
                    SourceLineId INTEGER NOT NULL,
                    UNIQUE(SourceType, SourceLineId)
                );
                ";
                await createNewCmd.ExecuteNonQueryAsync();

                // Copy data from old to new, mapping Id -> SourceLineId, SourceType=1 (Buy)
                var copyCmd = connection.CreateCommand();
                copyCmd.CommandText = @"
                INSERT INTO CostTrackingEntries
                    (TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes, SourceType, SourceLineId)
                SELECT TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes, 1 as SourceType, Id as SourceLineId
                FROM CostTrackingEntries_old;
                ";
                await copyCmd.ExecuteNonQueryAsync();

                // Drop old table
                var dropCmd = connection.CreateCommand();
                dropCmd.CommandText = "DROP TABLE IF EXISTS CostTrackingEntries_old";
                await dropCmd.ExecuteNonQueryAsync();

                // Create helpful index
                var indexCmd = connection.CreateCommand();
                indexCmd.CommandText = "CREATE INDEX IF NOT EXISTS IX_Cost_ItemDate ON CostTrackingEntries(ItemId, TransDate)";
                await indexCmd.ExecuteNonQueryAsync();
            }
            else if (tableMissing)
            {
                // Fresh create with new schema
                var createCmd = connection.CreateCommand();
                createCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS CostTrackingEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TransDate TEXT NOT NULL,
                    ItemId INTEGER NOT NULL,
                    DocType INTEGER NOT NULL,
                    QtyDelta DECIMAL(18,4) NOT NULL,
                    ValueDelta DECIMAL(18,4) NOT NULL,
                    QtyAfter DECIMAL(18,4) NOT NULL,
                    AvgCostAfter DECIMAL(18,6) NOT NULL,
                    SourceDocId INTEGER,
                    Notes TEXT,
                    SourceType INTEGER NOT NULL,
                    SourceLineId INTEGER NOT NULL,
                    UNIQUE(SourceType, SourceLineId)
                );
                ";
                await createCmd.ExecuteNonQueryAsync();

                var indexCmd = connection.CreateCommand();
                indexCmd.CommandText = "CREATE INDEX IF NOT EXISTS IX_Cost_ItemDate ON CostTrackingEntries(ItemId, TransDate)";
                await indexCmd.ExecuteNonQueryAsync();
            }
            else
            {
                // Table exists with new schema; ensure index exists
                var indexCmd = connection.CreateCommand();
                indexCmd.CommandText = "CREATE INDEX IF NOT EXISTS IX_Cost_ItemDate ON CostTrackingEntries(ItemId, TransDate)";
                await indexCmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error initializing/migrating CostTrackingEntries table");
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
        cmd.CommandText = @"SELECT Id, TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes, SourceType, SourceLineId FROM CostTrackingEntries ORDER BY TransDate, Id";
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
        cmd.CommandText = @"SELECT Id, TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes, SourceType, SourceLineId FROM CostTrackingEntries WHERE ItemId=@itemId AND TransDate>=@fromDate AND TransDate<=@toDate ORDER BY TransDate, Id";
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

    // public async Task<bool> EntryExists(int id)
    // {
    //     await Init();
    //     await using var connection = new SqliteConnection(Constants.DatabasePath);
    //     await connection.OpenAsync();
    //     var cmd = connection.CreateCommand();
    //     cmd.CommandText = "SELECT Id FROM CostTrackingEntries WHERE Id=@id";
    //     cmd.Parameters.AddWithValue("@id", id);
    //     await using var reader = await cmd.ExecuteReaderAsync();
    //     return await reader.ReadAsync();
    // }

    public async Task<bool> EntryExistsBySourceAsync(int sourceType, int sourceLineId)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id FROM CostTrackingEntries WHERE SourceType=@sourceType AND SourceLineId=@sourceLineId";
        cmd.Parameters.AddWithValue("@sourceType", sourceType);
        cmd.Parameters.AddWithValue("@sourceLineId", sourceLineId);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    public async Task<Models.CostTrackingEntryDto?> GetBySourceAsync(int sourceType, int sourceLineId)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT Id, TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes, SourceType, SourceLineId FROM CostTrackingEntries WHERE SourceType=@sourceType AND SourceLineId=@sourceLineId";
        cmd.Parameters.AddWithValue("@sourceType", sourceType);
        cmd.Parameters.AddWithValue("@sourceLineId", sourceLineId);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadDto(reader);
        }
        return null;
    }

    public async Task<Models.CostTrackingEntryDto?> GetAsync(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT Id, TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes, SourceType, SourceLineId FROM CostTrackingEntries WHERE Id=@id";
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
        cmd.CommandText = @"INSERT INTO CostTrackingEntries (TransDate, ItemId, DocType, QtyDelta, ValueDelta, QtyAfter, AvgCostAfter, SourceDocId, Notes, SourceType, SourceLineId) VALUES (@transDate, @itemId, @docType, @qtyDelta, @valueDelta, @qtyAfter, @avgCostAfter, @sourceDocId, @notes, @sourceType, @sourceLineId)";
        cmd.Parameters.AddWithValue("@transDate", entry.TransDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        cmd.Parameters.AddWithValue("@itemId", entry.ItemId);
        cmd.Parameters.AddWithValue("@docType", (int)entry.DocType);
        cmd.Parameters.AddWithValue("@qtyDelta", entry.QtyDelta);
        cmd.Parameters.AddWithValue("@valueDelta", entry.ValueDelta);
        cmd.Parameters.AddWithValue("@qtyAfter", qtyAfter);
        cmd.Parameters.AddWithValue("@avgCostAfter", avgCostAfter);
        cmd.Parameters.AddWithValue("@sourceDocId", entry.SourceDocId.HasValue ? entry.SourceDocId.Value : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@notes", entry.Notes ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@sourceType", entry.SourceType);
        cmd.Parameters.AddWithValue("@sourceLineId", entry.SourceLineId);
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
        cmd.CommandText = @"UPDATE CostTrackingEntries SET TransDate=@transDate, ItemId=@itemId, DocType=@docType, QtyDelta=@qtyDelta, ValueDelta=@valueDelta, QtyAfter=@qtyAfter, AvgCostAfter=@avgCostAfter, SourceDocId=@sourceDocId, Notes=@notes, SourceType=@sourceType, SourceLineId=@sourceLineId WHERE Id=@id";
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
        cmd.Parameters.AddWithValue("@sourceType", entry.SourceType);
        cmd.Parameters.AddWithValue("@sourceLineId", entry.SourceLineId);
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

    public async Task<decimal> GetAverageCostForDateAsync(int itemId, DateTime date)
    {
        await Init();
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT AvgCostAfter FROM CostTrackingEntries WHERE ItemId=@itemId AND TransDate>=@start AND TransDate<=@end ORDER BY TransDate DESC, Id DESC LIMIT 1";
        cmd.Parameters.AddWithValue("@itemId", itemId);
        cmd.Parameters.AddWithValue("@start", start);
        cmd.Parameters.AddWithValue("@end", end);
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                return reader.GetDecimal(0);
            }
        }
        // Fallback: return the latest available average cost if none found for the specified date
        cmd.Parameters.Clear();
        cmd.CommandText = @"SELECT AvgCostAfter FROM CostTrackingEntries WHERE ItemId=@itemId ORDER BY TransDate DESC, Id DESC LIMIT 1";
        cmd.Parameters.AddWithValue("@itemId", itemId);
        await using (var fallbackReader = await cmd.ExecuteReaderAsync())
        {
            if (await fallbackReader.ReadAsync())
            {
                return fallbackReader.GetDecimal(0);
            }
        }
        return 0m;
    }

    public async Task<int> AddPurchaseInvoiceAsync(int itemId, DateTime transDate, decimal quantity, decimal unitPrice, int? sourceDocId = null, string? notes = null)
    {
        var valueDelta = quantity * unitPrice; // net value
        var entry = new Models.CostTrackingEntryDto
        {
            Id = 0,
            TransDate = transDate,
            ItemId = itemId,
            DocType = Models.CostDocType.PurchaseInvoice,
            QtyDelta = quantity,
            ValueDelta = valueDelta,
            SourceDocId = sourceDocId,
            Notes = notes,
            SourceType = 0,
            SourceLineId = Guid.NewGuid().GetHashCode()
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
            Notes = notes,
            SourceType = 0,
            SourceLineId = Guid.NewGuid().GetHashCode()
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
            Notes = reader.IsDBNull(9) ? null : reader.GetString(9),
            SourceType = reader.FieldCount > 10 ? reader.GetInt32(10) : 1,
            SourceLineId = reader.FieldCount > 11 ? reader.GetInt32(11) : reader.GetInt32(0)
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
