using System;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using Pharmacy.Data.Models;

namespace Pharmacy.Data.Storage;

public class EmployeeStorage
{
    public static EmployeeStorage Instance = new EmployeeStorage();

    public async Task InsertAsync(Employee employee)
    {
        var db = Database.Instance;
        await db.OpenIfNeededAsync();

        using var command = db.Connection.CreateCommand();
        command.CommandText = "INSERT INTO employee (first_name, last_name, warehouse_id, role) VALUES (@firstName, @lastName, @warehouseId, @role)";
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

        Console.WriteLine();
    }
}