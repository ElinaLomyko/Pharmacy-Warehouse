using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Pharmacy.Data.Models;
using Pharmacy.Data.Storage;

namespace Pharmacy.Windows;

public partial class EditProductWindow : Window
{
    public ProductInfo? Item { get; set; }
    
    public EditProductWindow()
    {
        InitializeComponent();
        BtnCancel.Click += (sender, args) =>
        {
            Close();
        };

        BtnSave.Click += (sender, args) =>
        {
            _ = Save();
        };
    }

    public async Task Save()
    {
        if (Item == null) return;

        try
        {
            Item.Name = TbName.Text;
            Item.Count = int.Parse(TbCount.Text);

            await Database.Instance.UpdateProductAsync(Item);
            Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var item = Item;
        if (item == null) return;
        TbCount.Text = item.Count.ToString();
        TbName.Text = item.Name;
    }
}