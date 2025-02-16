using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.Data;

public class LocalBuyDocumentsRepo
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    public LocalBuyDocumentsRepo(ILogger<LocalBuyDocumentsRepo> logger)
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
           CREATE TABLE IF NOT EXISTS BuyDocuments (
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


       

            await createBuyDocTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating tables");
            throw;
        }

        _hasBeenInitialized = true;
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
   
}