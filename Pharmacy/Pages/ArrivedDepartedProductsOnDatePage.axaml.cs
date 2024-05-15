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

        CbForWeek.IsCheckedChanged += (sender, args) =>
        {
            _ = LoadData();
        };
    }

    private async Task LoadData()
    {
        var isArrival = CbType.SelectedIndex == 0;
        var startDate = DpDate.SelectedDate ?? DateTimeOffset.Now;
        var endDate = startDate + TimeSpan.FromDays(CbForWeek.IsChecked == true ? 7 : 1); 

        if (isArrival)
        {
            var result = await Database.Instance.GetArrivedProductsByDate(startDate, endDate);
            Dispatcher.UIThread.Invoke(() =>
            {
                DgData.ItemsSource = result;
                UpdateLayout();
            });
        }
        else
        {
            var result = await Database.Instance.GetDepartedProductsByDate(startDate, endDate);
            Dispatcher.UIThread.Invoke(() =>
            {
                DgData.ItemsSource = result;
                UpdateLayout();
            });
        }
    }
}