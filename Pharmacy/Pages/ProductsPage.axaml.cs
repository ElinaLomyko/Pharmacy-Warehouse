using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Pharmacy.Data.Storage;

namespace Pharmacy.Pages;

public partial class ProductsPage : UserControl
{
    private const string CountRunningOut = "30";

    public ProductsPage()
    {
        InitializeComponent();
        CbOnlyMissing.IsCheckedChanged += (_, _) =>
        {
            TbCount.Text = CbOnlyMissing.IsChecked == true ? "0" : null;
        };

        CbRunningOut.IsCheckedChanged += (_, _) =>
        {
            TbCount.Text = CbRunningOut.IsChecked == true ? CountRunningOut : null;
        };

        TbCount.TextChanged += (_, _) =>
        {
            switch (TbCount.Text)
            {
                case "0":
                {
                    if (CbOnlyMissing.IsChecked == false)
                    {
                        CbOnlyMissing.IsChecked = true;
                    }
                    CbRunningOut.IsChecked = false;
                    break;
                }
                case CountRunningOut:
                {
                    if (CbRunningOut.IsChecked == false)
                    {
                        CbRunningOut.IsChecked = true;
                    }
                    CbOnlyMissing.IsChecked = false;
                    break;
                }
                default:
                    CbOnlyMissing.IsChecked = false;
                    CbRunningOut.IsChecked = false;
                    break;
            }
        };
        
        BtnApply.Click += (sender, args) =>
        {
            _ = LoadData();
        };
    }

    private async Task LoadData()
    {
        int? count;
        if (int.TryParse(TbCount.Text ?? "", out var cnt))
        {
            count = cnt;
        }
        else
        {
            count = null;
        }
        
        var products = await Database.Instance.GetAllProducts(count);
        Dispatcher.UIThread.Invoke(() =>
        {
            DgProducts.ItemsSource = products;
        });
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = LoadData();
    }
}