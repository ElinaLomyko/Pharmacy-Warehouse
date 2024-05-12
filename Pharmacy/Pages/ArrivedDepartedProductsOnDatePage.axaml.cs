using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Pharmacy.Data.Storage;

namespace Pharmacy.Pages;

public partial class ArrivedDepartedProductsOnDatePage : UserControl
{
    public ArrivedDepartedProductsOnDatePage()
    {
        InitializeComponent();

        DpDate.SelectedDateChanged += (sender, args) =>
        {
            _ = LoadData();
        };

        CbType.SelectionChanged += (sender, args) =>
        {
            _ = LoadData();
        };
    }

    private async Task LoadData()
    {
        var isArrival = CbType.SelectedIndex == 0;
        var date = DpDate.SelectedDate ?? DateTimeOffset.Now;

        if (isArrival)
        {
            var result = await Database.Instance.GetArrivedProductsByDate(date);
            Dispatcher.UIThread.Invoke(() => { DgData.ItemsSource = result; });
        }
        else
        {
            var result = await Database.Instance.GetDepartedProductsByDate(date);
            Dispatcher.UIThread.Invoke(() => { DgData.ItemsSource = result; });
        }
    }
}