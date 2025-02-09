using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.Data;

public class LocalBuyDocumentsRepository
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    public LocalBuyDocumentsRepository(ILogger<LocalBuyDocumentsRepository> logger)
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
            var createBuyDocTableCmd = connection.CreateCommand();
            createBuyDocTableCmd.CommandText = @"
           CREATE TABLE BuyDocuments (
                Id INTEGER PRIMARY KEY NOT NULL,
                TransDate TEXT NOT NULL, -- DateTime stored as TEXT in ISO-8601 format (YYYY-MM-DDTHH:MM:SS.SSS).
                BuyDocDefId INTEGER NOT NULL,
                BuyDocDefName TEXT(200), -- Nullable (string?)
                SupplierId INTEGER NOT NULL,
                SupplierName TEXT(200), -- Nullable (string?)
                RefNumber TEXT(20), -- Nullable (string?)
                NetAmount DECIMAL(18, 2) NOT NULL, -- Decimal type handled as NUMERIC or DECIMAL in SQLite.
                VatAmount DECIMAL(18, 2) NOT NULL,
                TotalAmount DECIMAL(18, 2) NOT NULL
                );
           ";


            var createSuppliersTableCmd = connection.CreateCommand();
            createSuppliersTableCmd.CommandText = @"
                CREATE TABLE Suppliers (
                    Id INTEGER PRIMARY KEY,
                    Code TEXT(30) NOT NULL,
                    Name TEXT(200) NOT NULL,
                    Afm TEXT(10) NOT NULL
                );
                ";

            var createBuyDocLinesTableCmd = connection.CreateCommand();
            createBuyDocLinesTableCmd.CommandText = @"
              CREATE TABLE BuyDocLines (
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


            await createBuyDocTableCmd.ExecuteNonQueryAsync();
            await createSuppliersTableCmd.ExecuteNonQueryAsync();
            await createBuyDocLinesTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating tables");
            throw;
        }

        _hasBeenInitialized = true;
    }

    public async Task<List<SupplierListDto>> ListSuppliersAsync()
    {
        await Init(); // Ensure the database is initialized

        var suppliers = new List<SupplierListDto>();

        await using (var connection = new SqliteConnection(Constants.DatabasePath))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT Id, Code, Name, Afm 
            FROM Suppliers
        ";

            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    suppliers.Add(new SupplierListDto
                    {
                        Id = reader.GetInt32(0),
                        Code = reader.GetString(1),
                        Name = reader.GetString(2),
                        Afm = reader.GetString(3)
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error listing suppliers");
                throw;
            }
        }

        return suppliers;
    }

    public async Task<List<BuyDocListDto>> ListBuyDocsAsync()
    {
        await Init(); // Ensure the database is initialized

        var buyDocs = new List<BuyDocListDto>();

        await using (var connection = new SqliteConnection(Constants.DatabasePath))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT Id, TransDate, BuyDocDefId, BuyDocDefName, SupplierId, SupplierName, 
                   RefNumber, NetAmount, VatAmount, TotalAmount
            FROM  BuyDocuments
        ";

            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    buyDocs.Add(new BuyDocListDto
                    {
                        Id = reader.GetInt32(0),
                        TransDate = reader
                            .GetDateTime(1), // Assuming dates are stored in SQLite as ISO-8601 formatted TEXT.
                        BuyDocDefId = reader.GetInt32(2),
                        BuyDocDefName =
                            reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Handle nullable string
                        SupplierId = reader.GetInt32(4),
                        SupplierName =
                            reader.IsDBNull(5) ? string.Empty : reader.GetString(5), // Handle nullable string
                        RefNumber = reader.IsDBNull(6) ? string.Empty : reader.GetString(6), // Handle nullable string
                        NetAmount = reader.GetDecimal(7),
                        VatAmount = reader.GetDecimal(8),
                        TotalAmount = reader.GetDecimal(9)
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error listing BuyDocuments");
                throw;
            }
        }

        return buyDocs;
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

    public async Task<bool> SupplierExist(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT Id FROM Suppliers WHERE ID = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return true;
        }

        return false;
    }

    public async Task<bool> BuyDocumentExist(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT Id FROM BuyDocuments WHERE ID = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return true;
        }

        return false;
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

    public async Task<SupplierListDto?> GetSupplierAsync(int id)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, Code, Name, Afm
        FROM Suppliers
        WHERE Id = @id
    ";
        command.Parameters.AddWithValue("@id", id);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new SupplierListDto
                {
                    Id = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    Name = reader.GetString(2),
                    Afm = reader.GetString(3)
                };
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching supplier with Id = {Id}", id);
            throw;
        }

        return null; // Return null if no record is found
    }

    public async Task<BuyDocListDto?> GetBuyDocumentAsync(int id)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, TransDate, BuyDocDefId, BuyDocDefName, SupplierId, SupplierName, 
               RefNumber, NetAmount, VatAmount, TotalAmount
        FROM BuyDocuments
        WHERE Id = @id
    ";
        command.Parameters.AddWithValue("@id", id);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new BuyDocListDto
                {
                    Id = reader.GetInt32(0),
                    TransDate = reader.GetDateTime(1),
                    BuyDocDefId = reader.GetInt32(2),
                    BuyDocDefName = reader.IsDBNull(3) ? null : reader.GetString(3),
                    SupplierId = reader.GetInt32(4),
                    SupplierName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    RefNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                    NetAmount = reader.GetDecimal(7),
                    VatAmount = reader.GetDecimal(8),
                    TotalAmount = reader.GetDecimal(9)
                };
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching buy document with Id = {Id}", id);
            throw;
        }

        return null; // Return null if no record is found
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

    public async Task<int> AddSupplierAsync(SupplierListDto supplier)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Suppliers (Code, Name, Afm)
        VALUES (@code, @name, @afm);
    ";
        command.Parameters.AddWithValue("@code", supplier.Code);
        command.Parameters.AddWithValue("@name", supplier.Name);
        command.Parameters.AddWithValue("@afm", supplier.Afm);

        try
        {
            return await command.ExecuteNonQueryAsync(); // Returns the number of rows affected
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding supplier");
            throw;
        }
    }

    public async Task<int> AddBuyDocumentAsync(BuyDocListDto buyDoc)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO BuyDocuments (TransDate, BuyDocDefId, BuyDocDefName, SupplierId, SupplierName, 
                                   RefNumber, NetAmount, VatAmount, TotalAmount)
        VALUES (@transDate, @buyDocDefId, @buyDocDefName, @supplierId, @supplierName, 
                @refNumber, @netAmount, @vatAmount, @totalAmount);
    ";
        command.Parameters.AddWithValue("@transDate", buyDoc.TransDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        command.Parameters.AddWithValue("@buyDocDefId", buyDoc.BuyDocDefId);
        command.Parameters.AddWithValue("@buyDocDefName", buyDoc.BuyDocDefName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@supplierId", buyDoc.SupplierId);
        command.Parameters.AddWithValue("@supplierName", buyDoc.SupplierName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@refNumber", buyDoc.RefNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@netAmount", buyDoc.NetAmount);
        command.Parameters.AddWithValue("@vatAmount", buyDoc.VatAmount);
        command.Parameters.AddWithValue("@totalAmount", buyDoc.TotalAmount);

        try
        {
            return await command.ExecuteNonQueryAsync(); // Returns the number of rows affected
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding buy document");
            throw;
        }
    }

    public async Task<int> AddBuyDocLineAsync(BuyDocLineListDto buyDocLine)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO BuyDocLines (TransDate, BuyDocId, ItemId, ItemName, ItemCode, UnitOfMeasureName, 
                                    UnitFpaPerc, UnitQty, UnitPrice, UnitDiscountRate, UnitDiscountAmount, 
                                    UnitNetAmount, UnitVatAmount, UnitTotalAmount)
        VALUES (@transDate, @buyDocId, @itemId, @itemName, @itemCode, @unitOfMeasureName, 
                @unitFpaPerc, @unitQty, @unitPrice, @unitDiscountRate, @unitDiscountAmount, 
                @unitNetAmount, @unitVatAmount, @unitTotalAmount);
    ";
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

    public async Task<int> UpdateSupplierAsync(SupplierListDto supplier)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE Suppliers
        SET Code = @code, 
            Name = @name, 
            Afm = @afm
        WHERE Id = @id
    ";
        command.Parameters.AddWithValue("@id", supplier.Id);
        command.Parameters.AddWithValue("@code", supplier.Code);
        command.Parameters.AddWithValue("@name", supplier.Name);
        command.Parameters.AddWithValue("@afm", supplier.Afm);

        try
        {
            return await command.ExecuteNonQueryAsync(); // Returns the number of rows affected
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating supplier with Id = {Id}", supplier.Id);
            throw;
        }
    }

    public async Task<int> UpdateBuyDocumentAsync(BuyDocListDto buyDoc)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE BuyDocuments
        SET TransDate = @transDate,
            BuyDocDefId = @buyDocDefId,
            BuyDocDefName = @buyDocDefName,
            SupplierId = @supplierId,
            SupplierName = @supplierName,
            RefNumber = @refNumber,
            NetAmount = @netAmount,
            VatAmount = @vatAmount,
            TotalAmount = @totalAmount
        WHERE Id = @id
    ";
        command.Parameters.AddWithValue("@id", buyDoc.Id);
        command.Parameters.AddWithValue("@transDate", buyDoc.TransDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        command.Parameters.AddWithValue("@buyDocDefId", buyDoc.BuyDocDefId);
        command.Parameters.AddWithValue("@buyDocDefName", buyDoc.BuyDocDefName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@supplierId", buyDoc.SupplierId);
        command.Parameters.AddWithValue("@supplierName", buyDoc.SupplierName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@refNumber", buyDoc.RefNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@netAmount", buyDoc.NetAmount);
        command.Parameters.AddWithValue("@vatAmount", buyDoc.VatAmount);
        command.Parameters.AddWithValue("@totalAmount", buyDoc.TotalAmount);

        try
        {
            return await command.ExecuteNonQueryAsync(); // Returns the number of rows affected
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating buy document with Id = {Id}", buyDoc.Id);
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
    public async Task<int> DeleteSupplierAsync(SupplierListDto item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Suppliers WHERE ID = @id";
        deleteCmd.Parameters.AddWithValue("@id", item.Id);

        return await deleteCmd.ExecuteNonQueryAsync();
    }
    
    public async Task<int> DeleteBuyDocumentAsync(BuyDocListDto item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM BuyDocuments WHERE ID = @id";
        deleteCmd.Parameters.AddWithValue("@id", item.Id);

        return await deleteCmd.ExecuteNonQueryAsync();
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
}