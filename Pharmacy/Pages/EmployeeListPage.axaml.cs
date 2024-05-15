using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using Pharmacy.Data.Models;
using Pharmacy.Data.Storage;
using Pharmacy.Windows;

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

        DgEmployee.SelectionChanged += (sender, args) =>
        {
            BtnDelete.IsEnabled = DgEmployee.SelectedItem != null;
            BtnEdit.IsEnabled = DgEmployee.SelectedItem != null;
        };

        BtnDelete.Click += (sender, args) =>
        {
            _ = DeleteItem();
        };

        BtnEdit.Click += (sender, args) =>
        {
            _ = EditItem();
        };
        
        DgEmployee.DataContext = this;
    }


    private async Task EditItem()
    {
        
        var dialog = new EmployeeWindow
        {
            Item = (Employee)DgEmployee.SelectedItem
        };
        await dialog.ShowDialog((Window) VisualRoot);
        _ = LoadData();
    }

    private async Task DeleteItem()
    {
        var result =
            await MessageBoxManager.GetMessageBoxStandard(
                new MessageBoxStandardParams
                {
                    ContentTitle = "Вы уверены?",
                    ContentMessage = "Вы действительно хотите удалить сотрудника?",
                    ButtonDefinitions = ButtonEnum.YesNoCancel
                }).ShowWindowAsync();

        if (result.HasFlag(ButtonResult.Yes))
        {
            if (DgEmployee.SelectedItem is Employee e)
            {
                await Database.Instance.DeleteEmployeeAsync(e.Id);
                _ = LoadData();
            }
        }
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