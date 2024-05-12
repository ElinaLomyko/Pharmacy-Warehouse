using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using Pharmacy.Data.Models;

namespace Pharmacy.Data.Storage;

public class Database
{
    public static Database Instance = new Database();

    private MySqlConnection _connection = new MySqlConnection("server=localhost;uid=root;pwd=password;database=store");

    public MySqlConnection ConnectionReadOnly => _connection;
    

    public bool IsOpened => ConnectionReadOnly.State == ConnectionState.Open;


    private SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);
    private async Task<MySqlConnection> GetAndHoldConnectionAsync()
    {
        await _connectionSemaphore.WaitAsync();
        
        if (IsOpened)
        {
            return _connection;
        }

        try
        {
            await _connection.OpenAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        return _connection;
    }

    private void ReleaseConnection()
    {
        _connectionSemaphore.Release();
    }
    
    public async Task InsertEmployeeAsync(Employee employee)
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {

            using var command = connection.CreateCommand();
            command.CommandText =
                "INSERT INTO employee (first_name, last_name, warehouse_id, role) VALUES (@firstName, @lastName, @warehouseId, @role)";
            command.Parameters.Add("firstName", MySqlDbType.VarChar).Value = employee.FirstName;
            command.Parameters.Add("lastName", MySqlDbType.VarChar).Value = employee.LastName;
            command.Parameters.Add("warehouseId", MySqlDbType.Int32).Value = employee.WarehouseId;
            command.Parameters.Add("role", MySqlDbType.VarChar).Value = employee.Role;

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            command.Connection = null;
        }
        finally
        {
            ReleaseConnection();
        }
    }
    
    public async Task<List<Warehouse>> GetWarehousesAsync()
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM pharmacy_warehouse";
            await using var reader = await command.ExecuteReaderAsync();
            var result = new List<Warehouse>();
            while (reader.Read())
            {
                result.Add(new Warehouse
                {
                    Id = reader.GetInt32(0),
                    Address = reader.GetString(1)
                });
            }

            await reader.CloseAsync();
            command.Connection = null;
            return result;
        }
        finally
        {
            ReleaseConnection();
        }
    }

    public async Task<List<StorageCellFull>> GetAllStorageCellsAsync()
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText =
                "select * from storage_cell join store.pharmacy_warehouse pw on pw.warehouse_id = storage_cell.warehouse_id";
            await using var reader = await command.ExecuteReaderAsync();
            var result = new List<StorageCellFull>();
            while (reader.Read())
            {
                result.Add(new StorageCellFull
                {
                    StorageCellId = reader.GetInt32("storage_cell_id"),
                    Location = reader.GetString("location"),
                    Warehouse = new Warehouse
                    {
                        Id = reader.GetInt32("warehouse_id"),
                        Address = reader.GetString("address")
                    }
                });
            }

            await reader.CloseAsync();
            command.Connection = null;
            return result;
        }
        finally
        {
            ReleaseConnection();
        }
    }

    public async Task<decimal> GetCountArrivalsAsync()
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                                  SELECT SUM(scp.count) as total
                                  FROM store.arrival
                                  JOIN store.storage_cell_product scp ON scp.arrival_id = store.arrival.arrival_id
                                  """;

            var result = await command.ExecuteScalarAsync();
            command.Connection = null;
            return (decimal) result;
        }
        finally
        {
            ReleaseConnection();
        }
    }

    public async Task<decimal> GetCountDeparturesAsync()
    {
        var connection = GetAndHoldConnectionAsync();
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = """
                                  SELECT SUM(scp.count) as total
                                  FROM store.departure
                                  JOIN store.storage_cell_product scp ON scp.departure_id = store.departure.departure_id
                                  """;

            var result = await command.ExecuteScalarAsync();
            command.Connection = null;
            return (decimal)result;
        }
        finally
        {
            ReleaseConnection();
        }
    }

    public async Task<List<TopDeparted>> GetTopDepartedAsync()
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = """
                                  SELECT 
                                    top_departed.product_id,
                                    top_departed.total_departed,
                                    COALESCE (hp.name, m.name, me.name) as name
                                  FROM (
                                  	SELECT 
                                  	  product_id,
                                  	  sum(count) as total_departed
                                  	FROM store.storage_cell_product scp
                                  	WHERE scp.departure_id
                                  	GROUP BY (scp.product_id)
                                  	ORDER BY total_departed DESC
                                  	LIMIT 10
                                  ) as top_departed
                                  join store.product p on p.product_id = top_departed.product_id
                                  left join store.hygiene_product hp on hp.hygiene_product_id = p.hygiene_product_id 
                                  left join store.medicine m on m.medicine_id = p.medicine_id 
                                  left join store.medical_equipment me on me.medical_equipment_id = p.medical_equipment_id
                                  """;

            await using var reader = await command.ExecuteReaderAsync();
            var result = new List<TopDeparted>();

            while (reader.Read())
            {
                result.Add(new TopDeparted
                {
                    ProductId = reader.GetInt32("product_id"),
                    TotalDeparted = reader.GetDecimal("total_departed"),
                    ProductName = reader.GetString("name")
                });
            }

            await reader.CloseAsync();
            command.Connection = null;
            return result;
        }
        finally
        {
            ReleaseConnection();
        }
    }
    
    public async Task<List<CategoryDistribution>> GetCategoriesDistributionAsync()
    {
        var connection = GetAndHoldConnectionAsync();
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = """
                                  WITH totals AS (
                                    SELECT "hygiene_product" as category, SUM(hp.count) as cnt FROM store.hygiene_product hp
                                    UNION SELECT "medical_equipment" as category, SUM(me.count) as cnt FROM store.medical_equipment me
                                    UNION SELECT "medicine" as category, SUM(m.count) as cnt FROM store.medicine m
                                  ),
                                  total_cnt AS (
                                    SELECT SUM(cnt) as total_cnt FROM totals
                                  )
                                  SELECT 
                                    t.category,
                                    t.cnt,
                                    (t.cnt / tc.total_cnt) * 100 as percentage
                                  FROM totals t, total_cnt tc;
                                  """;


            await using var reader = await command.ExecuteReaderAsync();
            var result = new List<CategoryDistribution>();

            while (reader.Read())
            {
                result.Add(new CategoryDistribution
                {
                    Category = reader.GetString("category"),
                    Count = reader.GetDecimal("cnt"),
                    Percentage = reader.GetDecimal("percentage")
                });
            }

            await reader.CloseAsync();
            return result;
        }
        finally
        {
            ReleaseConnection();
        }
    }

    public async Task<List<ProductInfo>> GetAllProducts(int? countLessThan)
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                                  SELECT 
                                    p.product_id,
                                    COALESCE(hp.name, me.name, m.name) as name,
                                    COALESCE(hp.count, me.count, m.count) as count
                                  FROM store.product p 
                                  LEFT JOIN store.hygiene_product hp on hp.hygiene_product_id = p.hygiene_product_id
                                  LEFT JOIN store.medical_equipment me on me.medical_equipment_id = p.medical_equipment_id 
                                  LEFT JOIN store.medicine m on m.medicine_id = p.medicine_id
                                  """;

            if (countLessThan != null)
            {
                command.CommandText += $"\nWHERE COALESCE(hp.count, me.count, m.count) <= {countLessThan}";
            }
            
            await using var reader = await command.ExecuteReaderAsync();
            var result = new List<ProductInfo>();

            while (reader.Read())
            {
                result.Add(new ProductInfo
                {
                    Id = reader.GetInt32("product_id"),
                    Name = reader.GetString("name"),
                    Count = reader.GetInt32("count")
                });
            }

            await reader.CloseAsync();
            command.Connection = null;
            return result;
        }
        finally
        {
            ReleaseConnection();
        }
    }

    public async Task<List<ProductInfo>> GetArrivedProductsByDate(DateTimeOffset date)
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                                  SELECT
                                    p.product_id,
                                    COALESCE(hp.name, me.name, m.name) as name,
                                    scp.count,
                                    a.date_time
                                  FROM store.arrival a
                                  JOIN store.storage_cell_product scp on scp.arrival_id = a.arrival_id
                                  LEFT JOIN store.product p on p.product_id = scp.product_id 
                                  LEFT JOIN store.hygiene_product hp on hp.hygiene_product_id = p.hygiene_product_id
                                  LEFT JOIN store.medical_equipment me on me.medical_equipment_id = p.medical_equipment_id 
                                  LEFT JOIN store.medicine m on m.medicine_id = p.medicine_id
                                  WHERE a.date_time >= @dt_from and a.date_time <= @dt_to
                                  """;

            var dtFrom = date.Date;
            var dtTo = date.Date.AddDays(1);
            command.Parameters.Add("dt_from", MySqlDbType.Date).Value = dtFrom;
            command.Parameters.Add("dt_to", MySqlDbType.Date).Value = dtTo;
            await using var reader = await command.ExecuteReaderAsync();
            var result = new List<ProductInfo>();
            while (reader.Read())
            {
                result.Add(new ProductInfo
                {
                    Id = reader.GetInt32("product_id"),
                    Name = reader.GetString("name"),
                    Count = reader.GetInt32("count")
                });
            }

            await reader.CloseAsync();
            command.Connection = null;
            
            return result;
        }
        finally
        {
            ReleaseConnection();
        }
    }
    
    public async Task<List<ProductInfo>> GetDepartedProductsByDate(DateTimeOffset date)
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                                  SELECT
                                    p.product_id,
                                    COALESCE(hp.name, me.name, m.name) as name,
                                    scp.count,
                                    d.date_time
                                  FROM store.departure d
                                  JOIN store.storage_cell_product scp on scp.departure_id = d.departure_id
                                  LEFT JOIN store.product p on p.product_id = scp.product_id 
                                  LEFT JOIN store.hygiene_product hp on hp.hygiene_product_id = p.hygiene_product_id
                                  LEFT JOIN store.medical_equipment me on me.medical_equipment_id = p.medical_equipment_id 
                                  LEFT JOIN store.medicine m on m.medicine_id = p.medicine_id
                                  WHERE d.date_time >= @dt_from and d.date_time <= @dt_to
                                  """;

            var dtFrom = date.Date;
            var dtTo = date.Date.AddDays(1);
            command.Parameters.Add("dt_from", MySqlDbType.Date).Value = dtFrom;
            command.Parameters.Add("dt_to", MySqlDbType.Date).Value = dtTo;
            await using var reader = await command.ExecuteReaderAsync();
            var result = new List<ProductInfo>();
            while (reader.Read())
            {
                result.Add(new ProductInfo
                {
                    Id = reader.GetInt32("product_id"),
                    Name = reader.GetString("name"),
                    Count = reader.GetInt32("count")
                });
            }

            await reader.CloseAsync();
            command.Connection = null;
            
            return result;
        }
        finally
        {
            ReleaseConnection();
        }
    }

    public async Task<List<Employee>> GetAllEmployeeAsync()
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM employee JOIN store.pharmacy_warehouse pw on pw.warehouse_id = employee.warehouse_id";

            await using var reader = await command.ExecuteReaderAsync();
            var result = new List<Employee>();
            while (reader.Read())
            {
                result.Add(new Employee
                {
                    Id = reader.GetInt32("employee_id"),
                    FirstName = reader.GetString("first_name"),
                    LastName = reader.GetString("last_name"),
                    Role = reader.GetString("role"),
                    WarehouseId = reader.GetInt32("warehouse_id"),
                    Warehouse = new Warehouse
                    {
                        Id = reader.GetInt32("warehouse_id"),
                        Address = reader.GetString("address")
                    }
                });
            }

            await reader.CloseAsync();
            command.Connection = null;
            return result;
        }
        finally
        {
            ReleaseConnection();
        }
    }

    private static async Task InsertProductAsync(
        MySqlConnection connection,
        int? hygieneProductId = null,
        int? medicalEquipmentId = null,
        int? medicineId = null)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO product (hygiene_product_id, medical_equipment_id, medicine_id) VALUES (@hygiene_product_id, @medical_equipment_id, @medicine_id)";

        AddParameter("hygiene_product_id", hygieneProductId);
        AddParameter("medical_equipment_id", medicalEquipmentId);
        AddParameter("medicine_id", medicineId);

        await command.ExecuteNonQueryAsync();
        command.Connection = null;
        return;

        void AddParameter(string name, int? value)
        {
            if (value == null)
            {
                command.Parameters.Add(name, MySqlDbType.Null).Value = null;
            }
            else
            {
                command.Parameters.Add(name, MySqlDbType.Int32).Value = value.Value;
            }
        }
    }
    
    public async Task InsertHygieneProductAsync(ProductInfo productInfo)
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO hygiene_product (name, count) VALUES (@name, @count)";
            command.Parameters.Add("name", MySqlDbType.VarChar).Value = productInfo.Name;
            command.Parameters.Add("count", MySqlDbType.Int32).Value = productInfo.Count;
            await command.ExecuteNonQueryAsync();
            command.Connection = null;
            await InsertProductAsync(connection, hygieneProductId: (int) command.LastInsertedId);
        }
        finally
        {
            ReleaseConnection();
        }
    }
    
    public async Task InsertMedicineAsync(ProductInfo productInfo)
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO medicine (name, count) VALUES (@name, @count)";
            command.Parameters.Add("name", MySqlDbType.VarChar).Value = productInfo.Name;
            command.Parameters.Add("count", MySqlDbType.Int32).Value = productInfo.Count;
            await command.ExecuteNonQueryAsync();
            command.Connection = null;
            await InsertProductAsync(connection, medicineId: (int) command.LastInsertedId);
        }
        finally
        {
            ReleaseConnection();
        }
    }
    
    public async Task InsertMedicalEquipmentAsync(ProductInfo productInfo)
    {
        var connection = await GetAndHoldConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO medical_equipment (name, count) VALUES (@name, @count)";
            command.Parameters.Add("name", MySqlDbType.VarChar).Value = productInfo.Name;
            command.Parameters.Add("count", MySqlDbType.Int32).Value = productInfo.Count;
            await command.ExecuteNonQueryAsync();
            command.Connection = null;
            await InsertProductAsync(connection, medicalEquipmentId: (int) command.LastInsertedId);
        }
        finally
        {
            ReleaseConnection();
        }
    }
}