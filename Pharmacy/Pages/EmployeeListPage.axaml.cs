using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pharmacy.Data.Models;
using Pharmacy.Data.Storage;

namespace Pharmacy.Pages;

public partial class EmployeeListPage : UserControl
{
    // public ObservableCollection<Warehouse> Warehouses { get; set; } = [];
    
    public EmployeeListPage()
    {
        InitializeComponent();
        DataContext = this;
        DgEmployee.CurrentCellChanged += (sender, args) =>
        {
            Console.WriteLine();
        };
        
        DgEmployee.DataContext = this;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = LoadData();
    }

    public ObservableCollection<Employee> Employees { get; set; } = [];
    public ObservableCollection<Warehouse> Warehouses { get; set; } = [];
    
    private async Task LoadData()
    {
        var result = await Database.Instance.GetAllEmployeeAsync();
        Employees.Clear();
        foreach (var employee in result)
        {
            Employees.Add(employee);
        }
        var warehouses = result.Select(x => x.Warehouse).DistinctBy(x => x.Id);
        Warehouses.Clear();
        foreach (var wh in warehouses)
        {
            Warehouses.Add(wh);
        }
    }
}