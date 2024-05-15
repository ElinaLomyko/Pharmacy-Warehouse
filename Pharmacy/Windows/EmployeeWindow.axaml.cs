using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Pharmacy.Data.Models;
using Pharmacy.Data.Storage;

namespace Pharmacy.Windows;

public partial class EmployeeWindow : Window
{
    public Employee Item { get; set; }

    public EmployeeWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = LoadData();
        TbFirstName.Text = Item.FirstName;
        TbLastName.Text = Item.LastName;
        TbRole.Text = Item.Role;

        BtnCancel.Click += (sender, args) =>
        {
            Close();
        };

        BtnSave.Click += (sender, args) =>
        {
            _ = Save();
        };
    }

    private async Task LoadData()
    {
        var warehouses = await Database.Instance.GetWarehousesAsync();
        CbWarehouse.ItemsSource = warehouses;
        CbWarehouse.SelectedItem = warehouses.Find(x => x.Id == Item.WarehouseId);
    }

    private async Task Save()
    {
        Item.FirstName = TbFirstName.Text;
        Item.LastName = TbLastName.Text;
        Item.Role = TbRole.Text;
        Item.Warehouse = (Warehouse)CbWarehouse.SelectedItem;
        Item.WarehouseId = Item.Warehouse.Id;

        await Database.Instance.SaveEmployeeAsync(Item);
        Close();
    }
}