namespace Pharmacy.Data.Models;

public class StorageCellFull
{
    public int StorageCellId { get; set; }
    public string Location { get; set; }
    public Warehouse Warehouse { get; set; }
}