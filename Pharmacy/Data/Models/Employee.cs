namespace Pharmacy.Data.Models;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }
    public string Role { get; set; }
}