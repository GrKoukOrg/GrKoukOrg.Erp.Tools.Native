using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.Data;

public class LocalBuyDocLinesRepo
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    public LocalBuyDocLinesRepo(ILogger<LocalBuyDocLinesRepo> logger)
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
            var createBuyDocLinesTableCmd = connection.CreateCommand();
            createBuyDocLinesTableCmd.CommandText = @"
              CREATE TABLE IF NOT EXISTS BuyDocLines (
                Id INTEGER PRIMARY KEY,
                TransDate TEXT NOT NULL, -- DateTime in C# maps to TEXT in SQLite in ISO 8601 format
                BuyDocId INTEGER NOT NULL,
                ItemId INTEGER NOT NULL,
                ItemName TEXT(200), -- Nullable string
                ItemCode TEXT(20), -- Nullable string
                UnitOfMeasureName TEXT(200), -- Nullable string
                UnitFpaPerc REAL NOT NULL,
                UnitQty DECIMAL(18, 4) NOT NULL,
                UnitPrice DECIMAL(18, 4) NOT NULL, -- Decimal can be represented as REAL in SQLite
                UnitDiscountRate REAL NOT NULL,
                UnitDiscountAmount DECIMAL(18, 2) NOT NULL,
                UnitNetAmount DECIMAL(18, 2) NOT NULL,
                UnitVatAmount DECIMAL(18, 2) NOT NULL,
                UnitTotalAmount DECIMAL(18, 2) NOT NULL
            );
                ";


            await createBuyDocLinesTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating tables");
            throw;
        }

        _hasBeenInitialized = true;
    }

    public async Task<List<BuyDocLineListDto>> ListBuyDocLinesAsync()
    {
        await Init(); // Ensure the database is initialized

        var buyDocLines = new List<BuyDocLineListDto>();

        await using (var connection = new SqliteConnection(Constants.DatabasePath))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT Id, TransDate, BuyDocId, ItemId, ItemName, ItemCode, UnitOfMeasureName, 
                   UnitFpaPerc, UnitQty, UnitPrice, UnitDiscountRate, UnitDiscountAmount, 
                   UnitNetAmount, UnitVatAmount, UnitTotalAmount
            FROM  BuyDocLines
        ";

            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    buyDocLines.Add(new BuyDocLineListDto
                    {
                        Id = reader.GetInt32(0),
                        TransDate = reader.GetDateTime(1), // Assuming dates are stored as ISO-8601 formatted TEXT
                        BuyDocId = reader.GetInt32(2),
                        ItemId = reader.GetInt32(3),
                        ItemName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4), // Handle nullable string
                        ItemCode = reader.IsDBNull(5) ? string.Empty : reader.GetString(5), // Handle nullable string
                        UnitOfMeasureName =
                            reader.IsDBNull(6) ? string.Empty : reader.GetString(6), // Handle nullable string
                        UnitFpaPerc = reader.GetDouble(7),
                        UnitQty = reader.GetDecimal(8),
                        UnitPrice = reader.GetDecimal(9),
                        UnitDiscountRate = reader.GetDouble(10),
                        UnitDiscountAmount = reader.GetDecimal(11),
                        UnitNetAmount = reader.GetDecimal(12),
                        UnitVatAmount = reader.GetDecimal(13),
                        UnitTotalAmount = reader.GetDecimal(14)
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error listing BuyDocLines");
                throw;
            }
        }

        return buyDocLines;
    }

    public async Task<List<BuyDocLineListDto>> ListBuyDocLinesByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        await Init(); // Ensure the database is initialized

        var buyDocLines = new List<BuyDocLineListDto>();

        await using (var connection = new SqliteConnection(Constants.DatabasePath))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
        SELECT Id, TransDate, BuyDocId, ItemId, ItemName, ItemCode, UnitOfMeasureName, 
               UnitFpaPerc, UnitQty, UnitPrice, UnitDiscountRate, UnitDiscountAmount, 
               UnitNetAmount, UnitVatAmount, UnitTotalAmount
        FROM BuyDocLines
        WHERE TransDate >= @FromDate AND TransDate <= @ToDate
        ";
            command.Parameters.AddWithValue("@FromDate", fromDate);
            command.Parameters.AddWithValue("@ToDate", toDate);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    buyDocLines.Add(new BuyDocLineListDto
                    {
                        Id = reader.GetInt32(0),
                        TransDate = reader.GetDateTime(1),
                        BuyDocId = reader.GetInt32(2),
                        ItemId = reader.GetInt32(3),
                        ItemName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        ItemCode = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        UnitOfMeasureName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        UnitFpaPerc = reader.GetDouble(7),
                        UnitQty = reader.GetDecimal(8),
                        UnitPrice = reader.GetDecimal(9),
                        UnitDiscountRate = reader.GetDouble(10),
                        UnitDiscountAmount = reader.GetDecimal(11),
                        UnitNetAmount = reader.GetDecimal(12),
                        UnitVatAmount = reader.GetDecimal(13),
                        UnitTotalAmount = reader.GetDecimal(14)
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error listing BuyDocLines within the date range {FromDate} to {ToDate}", fromDate,
                    toDate);
                throw;
            }
        }

        return buyDocLines;
    }

    public async Task<bool> BuyDocLineExist(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT Id FROM BuyDocLines WHERE ID = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return true;
        }

        return false;
    }


    public async Task<BuyDocLineListDto?> GetBuyDocLineAsync(int id)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, TransDate, BuyDocId, ItemId, ItemName, ItemCode, UnitOfMeasureName, 
               UnitFpaPerc, UnitQty, UnitPrice, UnitDiscountRate, UnitDiscountAmount, 
               UnitNetAmount, UnitVatAmount, UnitTotalAmount
        FROM BuyDocLines
        WHERE Id = @id
    ";
        command.Parameters.AddWithValue("@id", id);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new BuyDocLineListDto
                {
                    Id = reader.GetInt32(0),
                    TransDate = reader.GetDateTime(1),
                    BuyDocId = reader.GetInt32(2),
                    ItemId = reader.GetInt32(3),
                    ItemName = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ItemCode = reader.IsDBNull(5) ? null : reader.GetString(5),
                    UnitOfMeasureName = reader.IsDBNull(6) ? null : reader.GetString(6),
                    UnitFpaPerc = reader.GetDouble(7),
                    UnitQty = reader.GetDecimal(8),
                    UnitPrice = reader.GetDecimal(9),
                    UnitDiscountRate = reader.GetDouble(10),
                    UnitDiscountAmount = reader.GetDecimal(11),
                    UnitNetAmount = reader.GetDecimal(12),
                    UnitVatAmount = reader.GetDecimal(13),
                    UnitTotalAmount = reader.GetDecimal(14)
                };
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching buy doc line with Id = {Id}", id);
            throw;
        }

        return null; // Return null if no record is found
    }


    public async Task<int> AddBuyDocLineAsync(BuyDocLineListDto buyDocLine)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO BuyDocLines (Id,TransDate, BuyDocId, ItemId, ItemName, ItemCode, UnitOfMeasureName, 
                                    UnitFpaPerc, UnitQty, UnitPrice, UnitDiscountRate, UnitDiscountAmount, 
                                    UnitNetAmount, UnitVatAmount, UnitTotalAmount)
        VALUES (@id,@transDate, @buyDocId, @itemId, @itemName, @itemCode, @unitOfMeasureName, 
                @unitFpaPerc, @unitQty, @unitPrice, @unitDiscountRate, @unitDiscountAmount, 
                @unitNetAmount, @unitVatAmount, @unitTotalAmount);
    ";
        command.Parameters.AddWithValue("@id", buyDocLine.Id);
        command.Parameters.AddWithValue("@transDate", buyDocLine.TransDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        command.Parameters.AddWithValue("@buyDocId", buyDocLine.BuyDocId);
        command.Parameters.AddWithValue("@itemId", buyDocLine.ItemId);
        command.Parameters.AddWithValue("@itemName", buyDocLine.ItemName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@itemCode", buyDocLine.ItemCode ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@unitOfMeasureName", buyDocLine.UnitOfMeasureName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@unitFpaPerc", buyDocLine.UnitFpaPerc);
        command.Parameters.AddWithValue("@unitQty", buyDocLine.UnitQty);
        command.Parameters.AddWithValue("@unitPrice", buyDocLine.UnitPrice);
        command.Parameters.AddWithValue("@unitDiscountRate", buyDocLine.UnitDiscountRate);
        command.Parameters.AddWithValue("@unitDiscountAmount", buyDocLine.UnitDiscountAmount);
        command.Parameters.AddWithValue("@unitNetAmount", buyDocLine.UnitNetAmount);
        command.Parameters.AddWithValue("@unitVatAmount", buyDocLine.UnitVatAmount);
        command.Parameters.AddWithValue("@unitTotalAmount", buyDocLine.UnitTotalAmount);

        try
        {
            return await command.ExecuteNonQueryAsync(); // Returns the number of rows affected
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding buy document line");
            throw;
        }
    }


    public async Task<int> UpdateBuyDocLineAsync(BuyDocLineListDto buyDocLine)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE BuyDocLines
        SET TransDate = @transDate,
            BuyDocId = @buyDocId,
            ItemId = @itemId,
            ItemName = @itemName,
            ItemCode = @itemCode,
            UnitOfMeasureName = @unitOfMeasureName,
            UnitFpaPerc = @unitFpaPerc,
            UnitQty = @unitQty,
            UnitPrice = @unitPrice,
            UnitDiscountRate = @unitDiscountRate,
            UnitDiscountAmount = @unitDiscountAmount,
            UnitNetAmount = @unitNetAmount,
            UnitVatAmount = @unitVatAmount,
            UnitTotalAmount = @unitTotalAmount
        WHERE Id = @id
    ";
        command.Parameters.AddWithValue("@id", buyDocLine.Id);
        command.Parameters.AddWithValue("@transDate", buyDocLine.TransDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        command.Parameters.AddWithValue("@buyDocId", buyDocLine.BuyDocId);
        command.Parameters.AddWithValue("@itemId", buyDocLine.ItemId);
        command.Parameters.AddWithValue("@itemName", buyDocLine.ItemName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@itemCode", buyDocLine.ItemCode ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@unitOfMeasureName", buyDocLine.UnitOfMeasureName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@unitFpaPerc", buyDocLine.UnitFpaPerc);
        command.Parameters.AddWithValue("@unitQty", buyDocLine.UnitQty);
        command.Parameters.AddWithValue("@unitPrice", buyDocLine.UnitPrice);
        command.Parameters.AddWithValue("@unitDiscountRate", buyDocLine.UnitDiscountRate);
        command.Parameters.AddWithValue("@unitDiscountAmount", buyDocLine.UnitDiscountAmount);
        command.Parameters.AddWithValue("@unitNetAmount", buyDocLine.UnitNetAmount);
        command.Parameters.AddWithValue("@unitVatAmount", buyDocLine.UnitVatAmount);
        command.Parameters.AddWithValue("@unitTotalAmount", buyDocLine.UnitTotalAmount);

        try
        {
            return await command.ExecuteNonQueryAsync(); // Returns the number of rows affected
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating buy document line with Id = {Id}", buyDocLine.Id);
            throw;
        }
    }


    public async Task<int> DeleteBuyDocLineAsync(BuyDocLineListDto item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM BuyDoclines WHERE ID = @id";
        deleteCmd.Parameters.AddWithValue("@id", item.Id);

        return await deleteCmd.ExecuteNonQueryAsync();
    }

    public async Task<int> DeleteAllBuyDocLineAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM BuyDoclines";

        return await deleteCmd.ExecuteNonQueryAsync();
    }
}