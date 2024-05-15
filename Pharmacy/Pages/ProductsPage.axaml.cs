using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using Pharmacy.Data.Models;
using Pharmacy.Data.Storage;
using Pharmacy.Windows;

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

        DgProducts.SelectionChanged += (sender, args) =>
        {
            SpEdit.IsVisible = DgProducts.SelectedItem != null;
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

        BtnDelete.Click += (sender, args) =>
        {
            _ = DeleteItem();
        };

        BtnEdit.Click += (sender, args) =>
        {
            _ = EditItem();
        };
    }

    private async Task EditItem()
    {
        
        var dialog = new EditProductWindow();
        dialog.Item = (ProductInfo)DgProducts.SelectedItem;
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
                    ContentMessage = "Вы действительно хотите удалить указанный товар?",
                    ButtonDefinitions = ButtonEnum.YesNoCancel
                }).ShowWindowAsync();

        if (result.HasFlag(ButtonResult.Yes))
        {
            if (DgProducts.SelectedItem is ProductInfo p)
            {
                await Database.Instance.DeleteProductAsync(p.Id);
                _ = LoadData();
            }
        }
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
            DgProducts.AutoGeneratingColumn += (sender, args) =>
            {
                if (args.PropertyType == typeof(int?))
                {
                    args.Cancel = true;
                }
            };
        });
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = LoadData();
    }
}