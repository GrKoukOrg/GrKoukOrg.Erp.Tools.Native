using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrKoukOrg.Erp.Tools.Native.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;

namespace GrKoukOrg.Erp.Tools.Native.Data;
/// <summary>
/// Repository class for managing Local Items (Warehouse Items) in the database.
/// </summary>
public class LocalItemsRepo
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalItemsRepo"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public LocalItemsRepo(ILogger<LocalItemsRepo> logger)
    {
        _logger = logger;
    }
     /// <summary>
        /// Initializes the database connection and creates the Items table if they do not exist.
        /// </summary>
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
            CREATE TABLE IF NOT EXISTS Items (
                ID INTEGER PRIMARY KEY,
                 Code               TEXT (30),
                Name               TEXT (200),
                MeasureUnitId      INTEGER,
                MeasureUnitName    TEXT (50),
                FpaCategoryId      INTEGER,
                FpaCategoryName    TEXT (50),
                Apothema           NUMERIC,
                TimiAgoras         NUMERIC,
                TimiAgorasFpa      NUMERIC,
                TimiPolisisLian    NUMERIC,
                TimiPolisisLianFpa NUMERIC,
                Barcodes           TEXT (1000)
            );";
                await createTableCmd.ExecuteNonQueryAsync();

                
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating items table");
                throw;
            }

            _hasBeenInitialized = true;
        }

        /// <summary>
        /// Retrieves a list of all Items from the database.
        /// </summary>
        /// <returns>A list of <see cref="ItemListDto"/> objects.</returns>
        public async Task<List<ItemListDto>> ListAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Items";
            var ItemListDtos = new List<ItemListDto>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ItemListDtos.Add(new ItemListDto
                {
                    Id = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    Name = reader.GetString(2),
                    MeasureUnitId = reader.GetInt32(3),
                    MeasureUnitName = reader.GetString(4),
                    FpaCategoryId = reader.GetInt32(5),
                    FpaCategoryName = reader.GetString(6),
                    Apothema = reader.GetDecimal(7),
                    TimiAgoras = reader.GetDecimal(8),
                    TimiAgorasFpa = reader.GetDecimal(9),
                    TimiPolisisLian = reader.GetDecimal(10),
                    TimiPolisisLianFpa = reader.GetDecimal(11),
                    Barcodes = reader.GetString(12),
                });
            }

            return ItemListDtos;
        }

        /// <summary>
        /// Checks if a specific Item with Id exists in the database.
        /// </summary>
        /// <param name="id">The ID of the ItemListDto.</param>
        /// <returns>A boolean true if exist false if not exist  </returns>
        public async Task<bool> ItemExist(int id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT Id FROM Items WHERE ID = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves a specific ItemListDto by its ID.
        /// </summary>
        /// <param name="id">The ID of the ItemListDto.</param>
        /// <returns>A <see cref="ItemListDto"/> object if found; otherwise, null.</returns>
        public async Task<ItemListDto?> GetAsync(int id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Items WHERE ID = @id";
            selectCmd.Parameters.AddWithValue("@id", id);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ItemListDto
                {
                    Id = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    Name = reader.GetString(2),
                    MeasureUnitId = reader.GetInt32(3),
                    MeasureUnitName = reader.GetString(4),
                    FpaCategoryId = reader.GetInt32(5),
                    FpaCategoryName = reader.GetString(6),
                    Apothema = reader.GetDecimal(7),
                    TimiAgoras = reader.GetDecimal(8),
                    TimiAgorasFpa = reader.GetDecimal(9),
                    TimiPolisisLian = reader.GetDecimal(10),
                    TimiPolisisLianFpa = reader.GetDecimal(11),
                    Barcodes = reader.GetString(12),
                };
            }

            return null;
        }

        /// <summary>
        /// Updates ItemListDto to the database. 
        /// </summary>
        /// <param name="item">The ItemListDto to save.</param>
        
        public async Task<int> UpdateItemAsync(ItemListDto item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
           
            saveCmd.CommandText = @"
                    UPDATE Items SET 
                      Code = @Code,
                       Name = @Name,
                       MeasureUnitId = @MeasureUnitId,
                       MeasureUnitName = @MeasureUnitName,
                       FpaCategoryId = @FpaCategoryId,
                       FpaCategoryName = @FpaCategoryName,
                       Apothema = @Apothema,
                       TimiAgoras = @TimiAgoras,
                       TimiAgorasFpa = @TimiAgorasFpa,
                       TimiPolisisLian = @TimiPolisisLian,
                       TimiPolisisLianFpa = @TimiPolisisLianFpa,
                       Barcodes = @Barcodes
                                 
                     WHERE ID = @Id
            ";
            saveCmd.Parameters.AddWithValue("@Id", item.Id);
            saveCmd.Parameters.AddWithValue("@Code", item.Code);
            saveCmd.Parameters.AddWithValue("@Name", item.Name);
            saveCmd.Parameters.AddWithValue("@MeasureUnitId", item.MeasureUnitId);
            saveCmd.Parameters.AddWithValue("@MeasureUnitName", item.MeasureUnitName);
            saveCmd.Parameters.AddWithValue("@FpaCategoryId", item.FpaCategoryId);
            saveCmd.Parameters.AddWithValue("@FpaCategoryName", item.FpaCategoryName);
            saveCmd.Parameters.AddWithValue("@Apothema", item.Apothema);
            saveCmd.Parameters.AddWithValue("@TimiAgoras", item.TimiAgoras);
            saveCmd.Parameters.AddWithValue("@TimiAgorasFpa", item.TimiAgorasFpa);
            saveCmd.Parameters.AddWithValue("@TimiPolisisLian", item.TimiPolisisLian);
            saveCmd.Parameters.AddWithValue("@TimiPolisisLianFpa", item.TimiPolisisLianFpa);
            saveCmd.Parameters.AddWithValue("@Barcodes", item.Barcodes);

            var result = await saveCmd.ExecuteScalarAsync();
            if (result != null)
            {
                _logger.LogDebug($"Item with Id {item.Id.ToString()} has been updated");
                return 1;
            }
            else
            {
                _logger.LogDebug(
                    $"Item with id {item.Id.ToString()} and name {item.Name} returned a null value result");
                return 0;
            }

            return item.Id;
        }
 /// <summary>
        /// Saves a ItemListDto to the database. If the ItemListDto ID is 0, a new ItemListDto is created; otherwise, the existing ItemListDto is updated.
        /// </summary>
        /// <param name="item">The ItemListDto to save.</param>
        /// <returns>The ID of the saved ItemListDto.</returns>
        public async Task<int> SaveItemAsync(ItemListDto item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            if (item.Id == 0)
            {
                saveCmd.CommandText = @"
                INSERT INTO Items (
                                      Id,
                                      Code,
                                      Name,
                                      MeasureUnitId,
                                      MeasureUnitName,
                                      FpaCategoryId,
                                      FpaCategoryName,
                                      Apothema,
                                      TimiAgoras,
                                      TimiAgorasFpa,
                                      TimiPolisisLian,
                                      TimiPolisisLianFpa,
                                      Barcodes
                                  )
                 VALUES (
                                      @Id,
                                      @Code,
                                      @Name,
                                      @MeasureUnitId,
                                      @MeasureUnitName,
                                      @FpaCategoryId,
                                      @FpaCategoryName,
                                      @Apothema,
                                      @TimiAgoras,
                                      @TimiAgorasFpa,
                                      @TimiPolisisLian,
                                      @TimiPolisisLianFpa,
                                      @Barcodes  
                                  );

                  SELECT last_insert_rowid();";
            }
            else
            {
                saveCmd.CommandText = @"
                    UPDATE Items SET 
                      Code = @Code,
                       Name = @Name,
                       MeasureUnitId = @MeasureUnitId,
                       MeasureUnitName = @MeasureUnitName,
                       FpaCategoryId = @FpaCategoryId,
                       FpaCategoryName = @FpaCategoryName,
                       Apothema = @Apothema,
                       TimiAgoras = @TimiAgoras,
                       TimiAgorasFpa = @TimiAgorasFpa,
                       TimiPolisisLian = @TimiPolisisLian,
                       TimiPolisisLianFpa = @TimiPolisisLianFpa,
                       Barcodes = @Barcodes
                                 
                     WHERE ID = @Id
            ";
                
                
                saveCmd.Parameters.AddWithValue("@Id", item.Id);
            }

            saveCmd.Parameters.AddWithValue("@Code", item.Code);
            saveCmd.Parameters.AddWithValue("@Name", item.Name);
            saveCmd.Parameters.AddWithValue("@MeasureUnitId", item.MeasureUnitId);
            saveCmd.Parameters.AddWithValue("@MeasureUnitName", item.MeasureUnitName);
            saveCmd.Parameters.AddWithValue("@FpaCategoryId", item.FpaCategoryId);
            saveCmd.Parameters.AddWithValue("@FpaCategoryName", item.FpaCategoryName);
            saveCmd.Parameters.AddWithValue("@Apothema", item.Apothema);
            saveCmd.Parameters.AddWithValue("@TimiAgoras", item.TimiAgoras);
            saveCmd.Parameters.AddWithValue("@TimiAgorasFpa", item.TimiAgorasFpa);
            saveCmd.Parameters.AddWithValue("@TimiPolisisLian", item.TimiPolisisLian);
            saveCmd.Parameters.AddWithValue("@TimiPolisisLianFpa", item.TimiPolisisLianFpa);
            saveCmd.Parameters.AddWithValue("@Barcodes", item.Barcodes);

            var result = await saveCmd.ExecuteScalarAsync();
            if (item.Id == 0)
            {
                item.Id = Convert.ToInt32(result);
            }

            return item.Id;
        }
       /// <summary>
        /// Adds a ItemListDto to the database. 
        /// </summary>
        /// <param name="item">The ItemListDto to Add.</param>
        
        public async Task<int> AddItemAsync(ItemListDto item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();

            saveCmd.CommandText = @"
                INSERT INTO Items (
                                      Id,
                                      Code,
                                      Name,
                                      MeasureUnitId,
                                      MeasureUnitName,
                                      FpaCategoryId,
                                      FpaCategoryName,
                                      Apothema,
                                      TimiAgoras,
                                      TimiAgorasFpa,
                                      TimiPolisisLian,
                                      TimiPolisisLianFpa,
                                      Barcodes
                                  )
                 VALUES (
                                      @Id,
                                      @Code,
                                      @Name,
                                      @MeasureUnitId,
                                      @MeasureUnitName,
                                      @FpaCategoryId,
                                      @FpaCategoryName,
                                      @Apothema,
                                      @TimiAgoras,
                                      @TimiAgorasFpa,
                                      @TimiPolisisLian,
                                      @TimiPolisisLianFpa,
                                      @Barcodes
                                  );
                ";
           
            saveCmd.Parameters.AddWithValue("@Id", item.Id);
            saveCmd.Parameters.AddWithValue("@Code", item.Code);
            saveCmd.Parameters.AddWithValue("@Name", item.Name);
            saveCmd.Parameters.AddWithValue("@MeasureUnitId", item.MeasureUnitId);
            saveCmd.Parameters.AddWithValue("@MeasureUnitName", item.MeasureUnitName);
            saveCmd.Parameters.AddWithValue("@FpaCategoryId", item.FpaCategoryId);
            saveCmd.Parameters.AddWithValue("@FpaCategoryName", item.FpaCategoryName);
            saveCmd.Parameters.AddWithValue("@Apothema", item.Apothema);
            saveCmd.Parameters.AddWithValue("@TimiAgoras", item.TimiAgoras);
            saveCmd.Parameters.AddWithValue("@TimiAgorasFpa", item.TimiAgorasFpa);
            saveCmd.Parameters.AddWithValue("@TimiPolisisLian", item.TimiPolisisLian);
            saveCmd.Parameters.AddWithValue("@TimiPolisisLianFpa", item.TimiPolisisLianFpa);
            saveCmd.Parameters.AddWithValue("@Barcodes", item.Barcodes);

            var result = await saveCmd.ExecuteScalarAsync();
            if (result != null)
            {
                _logger.LogDebug($"Item with Id {item.Id.ToString()} has been added to the database");
                return 1;
            }
            else
            {
                _logger.LogDebug(
                    $"Item with id {item.Id.ToString()} and name {item.Name} returned a null value result");
                return 0;
            }
            
        }

        /// <summary>
        /// Deletes a ItemListDto from the database.
        /// </summary>
        /// <param name="item">The ItemListDto to delete.</param>
        /// <returns>The number of rows affected.</returns>
        public async Task<int> DeleteItemAsync(ItemListDto item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Items WHERE ID = @id";
            deleteCmd.Parameters.AddWithValue("@id", item.Id);

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteAllItemsAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Items";
           

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Drops the ItemListDto and ProjectsItemListDtos tables from the database.
        /// </summary>
        public async Task DropTableAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var dropTableCmd = connection.CreateCommand();
            dropTableCmd.CommandText = "DROP TABLE IF EXISTS Items";
            await dropTableCmd.ExecuteNonQueryAsync();

           

            _hasBeenInitialized = false;
        }

        public async Task<ItemLocalStatisticsDto> GetLocalItemStatisticsAsync(int id)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Items WHERE ID = @id";
            selectCmd.Parameters.AddWithValue("@id", id);
            return new ItemLocalStatisticsDto();
        }
}