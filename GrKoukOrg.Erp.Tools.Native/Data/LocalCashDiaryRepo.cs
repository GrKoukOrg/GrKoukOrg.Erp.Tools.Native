using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.Data;

public class LocalCashDiaryDbRepo : ILocalCashDiaryRepo<CashDiaryItemDto>
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    public LocalCashDiaryDbRepo(ILogger<LocalCashDiaryDbRepo> logger)
    {
        _logger = logger;
    }

    public async Task Init()
    {
        if (_hasBeenInitialized)
            return;

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        try
        {
            var createSaleDocTableCmd = connection.CreateCommand();
            createSaleDocTableCmd.CommandText = @"
           CREATE TABLE IF NOT EXISTS SaleDocuments (
                Id INTEGER PRIMARY KEY NOT NULL,
                TransDate TEXT NOT NULL, -- DateTime stored as TEXT in ISO-8601 format (YYYY-MM-DDTHH:MM:SS.SSS).
                SaleDocDefId INTEGER NOT NULL,
                SaleDocDefName TEXT(200), -- Nullable (string?)
                CustomerId INTEGER NOT NULL,
                CustomerName TEXT(200), -- Nullable (string?)
                RefNumber TEXT(20), -- Nullable (string?)
                NetAmount DECIMAL(18, 2) NOT NULL, -- Decimal type handled as NUMERIC or DECIMAL in SQLite.
                VatAmount DECIMAL(18, 2) NOT NULL,
                TotalAmount DECIMAL(18, 2) NOT NULL
                );
           ";


       

            await createSaleDocTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating tables");
            throw;
        }

        _hasBeenInitialized = true;
    }

   

    public async Task<List<CashDiaryItemDto>> ListItemsAsync()
    {
        await Init(); // Ensure the database is initialized

        var saleDocs = new List<CashDiaryItemDto>();

        await using (var connection = new SqliteConnection(Constants.DatabasePath))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT Id, TransDate, SaleDocDefId, SaleDocDefName, CustomerId, CustomerName, 
                   RefNumber, NetAmount, VatAmount, TotalAmount
            FROM  SaleDocuments
        ";

            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    saleDocs.Add(new CashDiaryItemDto
                    {
                        Id = reader.GetInt32(0),
                        TransDate = reader
                            .GetDateTime(1), // Assuming dates are stored in SQLite as ISO-8601 formatted TEXT.
                        TransId = reader.GetInt32(2),
                        TransName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Handle nullable string
                        Etiologia = reader.IsDBNull(4) ? string.Empty : reader.GetString(4), // Handle nullable string
                        NetAmount = reader.GetDecimal(5),
                        VatAmount = reader.GetDecimal(6),
                        TotalAmount = reader.GetDecimal(7)
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error listing CashDiaryItems");
                throw;
            }
        }

        return saleDocs;
    }

   

    public async Task<bool> ItemExist(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT Id FROM SaleDocuments WHERE ID = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return true;
        }

        return false;
    }

    public async Task<CashDiaryItemDto?> GetItemAsync(int id)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, TransDate, SaleDocDefId, SaleDocDefName, CustomerId, CustomerName, 
               RefNumber, NetAmount, VatAmount, TotalAmount
        FROM SaleDocuments
        WHERE Id = @id
    ";
        command.Parameters.AddWithValue("@id", id);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CashDiaryItemDto
                {
                    Id = reader.GetInt32(0),
                    TransDate = reader
                        .GetDateTime(1), // Assuming dates are stored in SQLite as ISO-8601 formatted TEXT.
                    TransId = reader.GetInt32(2),
                    TransName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Handle nullable string
                    Etiologia = reader.IsDBNull(4) ? string.Empty : reader.GetString(4), // Handle nullable string
                    NetAmount = reader.GetDecimal(5),
                    VatAmount = reader.GetDecimal(6),
                    TotalAmount = reader.GetDecimal(7)
                };
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching sale document with Id = {Id}", id);
            throw;
        }

        return null; // Return null if no record is found
    }


    public async Task<int> AddItemAsync(CashDiaryItemDto saleDoc)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO SaleDocuments (Id,TransDate, SaleDocDefId, SaleDocDefName, CustomerId, CustomerName, 
                                   RefNumber, NetAmount, VatAmount, TotalAmount)
        VALUES (@Id,@transDate, @saleDocDefId, @saleDocDefName, @customerId, @customerName, 
                @refNumber, @netAmount, @vatAmount, @totalAmount);
    ";
        command.Parameters.AddWithValue("@Id", saleDoc.Id);
        command.Parameters.AddWithValue("@transDate", saleDoc.TransDate.ToString("yyyy-MM-ddTHH:mm:ss"));
       // command.Parameters.AddWithValue("@saleDocDefId", saleDoc.SaleDocDefId);
       // command.Parameters.AddWithValue("@saleDocDefName", saleDoc.SaleDocDefName ?? (object)DBNull.Value);
      //  command.Parameters.AddWithValue("@customerId", saleDoc.CustomerId);
      //  command.Parameters.AddWithValue("@customerName", saleDoc.CustomerName ?? (object)DBNull.Value);
      //  command.Parameters.AddWithValue("@refNumber", saleDoc.RefNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@netAmount", saleDoc.NetAmount);
        command.Parameters.AddWithValue("@vatAmount", saleDoc.VatAmount);
        command.Parameters.AddWithValue("@totalAmount", saleDoc.TotalAmount);

        try
        {
            return await command.ExecuteNonQueryAsync(); // Returns the number of rows affected
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding sale document");
            throw;
        }
    }

 

    public async Task<int> UpdateItemAsync(CashDiaryItemDto saleDoc)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE SaleDocuments
        SET TransDate = @transDate,
            SaleDocDefId = @saleDocDefId,
            SaleDocDefName = @saleDocDefName,
            CustomerId = @customerId,
            CustomerName = @customerName,
            RefNumber = @refNumber,
            NetAmount = @netAmount,
            VatAmount = @vatAmount,
            TotalAmount = @totalAmount
        WHERE Id = @id
    ";
        command.Parameters.AddWithValue("@id", saleDoc.Id);
        command.Parameters.AddWithValue("@transDate", saleDoc.TransDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        // command.Parameters.AddWithValue("@saleDocDefId", saleDoc.SaleDocDefId);
        // command.Parameters.AddWithValue("@saleDocDefName", saleDoc.SaleDocDefName ?? (object)DBNull.Value);
        // command.Parameters.AddWithValue("@customerId", saleDoc.CustomerId);
        // command.Parameters.AddWithValue("@customerName", saleDoc.CustomerName ?? (object)DBNull.Value);
        // command.Parameters.AddWithValue("@refNumber", saleDoc.RefNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@netAmount", saleDoc.NetAmount);
        command.Parameters.AddWithValue("@vatAmount", saleDoc.VatAmount);
        command.Parameters.AddWithValue("@totalAmount", saleDoc.TotalAmount);

        try
        {
            return await command.ExecuteNonQueryAsync(); // Returns the number of rows affected
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating sale document with Id = {Id}", saleDoc.Id);
            throw;
        }
    }

  
    
    public async Task<int> DeleteItemAsync(CashDiaryItemDto item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM SaleDocuments WHERE ID = @id";
        deleteCmd.Parameters.AddWithValue("@id", item.Id);

        return await deleteCmd.ExecuteNonQueryAsync();
    }
    public async Task<int> DeleteAllItemsAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM SaleDocuments ";

        return await deleteCmd.ExecuteNonQueryAsync();
    }
}