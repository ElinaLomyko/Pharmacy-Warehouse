namespace Pharmacy.Data.Models;

public class ProductInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Count { get; set; }
    
    public int? HygieneProductId { get; set; }
    public int? MedicalEquipmentId { get; set; }
    public int? MedicineId { get; set; }
}