using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GrKoukOrg.Erp.Tools.Native.Data;

public class LocalCustomerRepo
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    public LocalCustomerRepo(ILogger<LocalCustomerRepo> logger)
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
           
            var createCustomersTableCmd = connection.CreateCommand();
            createCustomersTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    Id INTEGER PRIMARY KEY,
                    Code TEXT(30) NOT NULL,
                    Name TEXT(200) NOT NULL,
                    Afm TEXT(10) NOT NULL
                );
                ";

            await createCustomersTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating tables");
            throw;
        }

        _hasBeenInitialized = true;
    }

    public async Task<List<CustomerListDto>> ListCustomersAsync()
    {
        await Init(); // Ensure the database is initialized

        var suppliers = new List<CustomerListDto>();

        await using (var connection = new SqliteConnection(Constants.DatabasePath))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT Id, Code, Name, Afm 
            FROM Customers
        ";

            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    suppliers.Add(new CustomerListDto
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

   


    public async Task<bool> CustomerExist(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT Id FROM Customers WHERE ID = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return true;
        }

        return false;
    }

   

    public async Task<CustomerListDto?> GetCustomerAsync(int id)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Id, Code, Name, Afm
        FROM Customers
        WHERE Id = @id
    ";
        command.Parameters.AddWithValue("@id", id);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CustomerListDto
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

  

    public async Task<int> AddCustomerAsync(CustomerListDto supplier)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Customers (Id,Code, Name, Afm)
        VALUES (@id,@code, @name, @afm);
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
            _logger.LogError(e, "Error adding supplier");
            throw;
        }
    }

    public async Task<int> UpdateCustomerAsync(CustomerListDto supplier)
    {
        await Init(); // Ensure the database is initialized

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE Customers
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

   
    public async Task<int> DeleteCustomerAsync(CustomerListDto item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Customers WHERE ID = @id";
        deleteCmd.Parameters.AddWithValue("@id", item.Id);

        return await deleteCmd.ExecuteNonQueryAsync();
    }
    public async Task<int> DeleteAllCustomersAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Customers";

        return await deleteCmd.ExecuteNonQueryAsync();
    }
  
    
}