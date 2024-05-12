using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Pharmacy.Data.Models;
using Pharmacy.Data.Storage;

namespace Pharmacy.Pages;

public partial class NewArrivalPage : UserControl
{
    public NewArrivalPage()
    {
        InitializeComponent();
        BtnAdd.Click += (sender, args) =>
        {
            _ = Add();
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = LoadData();
    }

    private async Task Add()
    {
        if (CbCell.SelectedItem == null) return;
        if (CbProduct.SelectedItem == null) return;
        if (DpArrival.SelectedDate == null) return;
        if (!int.TryParse(TbCount.Text, out var count)) return;

        await Database.Instance.InsertArrivalAsync(
            ((ProductInfo)CbProduct.SelectedItem).Id,
            ((StorageCellFull)CbCell.SelectedItem).StorageCellId,
            count, DpArrival.SelectedDate.Value);

        Dispatcher.UIThread.Invoke(() =>
        {
            CbCell.SelectedItem = null;
            CbProduct.SelectedItem = null;
            DpArrival.SelectedDate = null;
            TbCount.Text = null;
        });
    }

    private async Task LoadData()
    {
        var products = await Database.Instance.GetAllProducts(null);
        var cells = await Database.Instance.GetAllStorageCellsAsync();

        Dispatcher.UIThread.Invoke(() =>
        {
            CbProduct.ItemsSource = products;
            CbCell.ItemsSource = cells;
        });
    }
}