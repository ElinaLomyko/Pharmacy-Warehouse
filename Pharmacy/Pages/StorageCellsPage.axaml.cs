using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Pharmacy.Data.Storage;

namespace Pharmacy.Pages;

public partial class StorageCellsPage : UserControl
{
    public StorageCellsPage()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Database.Instance.GetAllStorageCellsAsync().ContinueWith(result =>
        {
            Dispatcher.UIThread.Invoke(() => {
                DgStorageCells.ItemsSource = result.Result.Select(x => new
                {
                    Id = x.StorageCellId,
                    Location = x.Location,
                    Warehouse = x.Warehouse.Address
                });
            });
        });
    }
}