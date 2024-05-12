using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Pharmacy.Data.Models;
using Pharmacy.Data.Storage;

namespace Pharmacy.Pages;

public partial class AddProductPage : UserControl
{
    public AddProductPage()
    {
        InitializeComponent();

        BtnAdd.Click += (sender, args) =>
        {
            _ = AddProduct();
        };
    }

    private async Task AddProduct()
    {
        if (string.IsNullOrEmpty(TbName.Text))
        {
            return;
        }

        if (!int.TryParse(TbCount.Text, out var count))
        {
            return;
        }

        var product = new ProductInfo
        {
            Name = TbName.Text,
            Count = count
        };

        try
        {
            if (CbType.SelectedIndex == 0)
            {
                await Database.Instance.InsertHygieneProductAsync(product);
            }
            else if (CbType.SelectedIndex == 1)
            {
                await Database.Instance.InsertMedicalEquipmentAsync(product);
            }
            else
            {
                await Database.Instance.InsertMedicineAsync(product);
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                TbName.Text = "";
                TbCount.Text = "";
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}