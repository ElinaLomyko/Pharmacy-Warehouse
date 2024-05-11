using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Pharmacy.Data.Models;

namespace Pharmacy.Data.Storage;

public class WarehouseStorage
{
    public static WarehouseStorage Instance = new WarehouseStorage();

    public async Task<List<Warehouse>> GetWarehouses()
    {
        await Database.Instance.OpenIfNeededAsync();
        
        await using var command = Database.Instance.Connection.CreateCommand();
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

        return result;
    }
}