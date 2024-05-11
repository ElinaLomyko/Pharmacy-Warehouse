using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Pharmacy.Data.Models;
using Pharmacy.Data.Storage;

namespace Pharmacy.Pages;

public partial class AddEmployeePage : UserControl
{
    public AddEmployeePage()
    {
        InitializeComponent();
        
        BtnAdd.Click += (_, _) =>
        {
            InsertEmployee();
        };
    }

    private void InsertEmployee()
    {
        var emp = new Employee
        {
            FirstName = TbFirstName.Text,
            LastName = TbLastName.Text,
            Role = TbRole.Text,
            WarehouseId = ((Warehouse) CbWarehouse.SelectedItem).Id
        };

        BtnAdd.Content = "Добавление...";
        Database.Instance.InsertEmployeeAsync(emp).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Invoke(() => {
                BtnAdd.Content = "Добавить";
                TbFirstName.Text = "";
                TbLastName.Text = "";
                TbRole.Text = "";
                CbWarehouse.SelectedItem = null;
                MainWindow.NotificationManager.Show(new Notification(
                    "Операция выполнена", "Сотрудник добавлен в базу данных!"));
            });
        });
    }

    private void LoadWarehouses()
    {
        Database.Instance.GetWarehousesAsync().ContinueWith(result =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                CbWarehouse.ItemsSource = result.Result;
            });
        });
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        LoadWarehouses();
    }
}