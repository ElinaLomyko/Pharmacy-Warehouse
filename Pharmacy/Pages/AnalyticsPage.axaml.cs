using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Pharmacy.Data.Storage;

namespace Pharmacy.Pages;

public partial class AnalyticsPage : UserControl
{
    public AnalyticsPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = LoadData();
    }

    private async Task LoadData()
    {
        var arrivals = await Database.Instance.GetCountArrivalsAsync();
        var departures = await Database.Instance.GetCountDeparturesAsync();
        var topDeparted = await Database.Instance.GetTopDepartedAsync();
        var categories = await Database.Instance.GetCategoriesDistributionAsync();
    
        Dispatcher.UIThread.Invoke(() =>
        {
            LbCountArrivals.Content = "Количество поставок: " + arrivals;
            LbCountDepartures.Content = "Количество отправлений: " + departures;
            DgDeparted.ItemsSource = topDeparted;
            DgPercentCategory.ItemsSource = categories;
        });
    }
}