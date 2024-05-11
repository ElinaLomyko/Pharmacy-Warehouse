using System;
using System.Data;
using Avalonia.Controls;
using Avalonia.Threading;
using Pharmacy.Storage;

namespace Pharmacy;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void UpdateConnectionStatus()
    {
        var state = Database.Connection.State;
        var status = state switch
        {
            ConnectionState.Closed => "Не подключено",
            ConnectionState.Connecting => "Подключение",
            ConnectionState.Executing => "Выполнение",
            ConnectionState.Fetching => "Получение данных",
            ConnectionState.Open => "Подключено",
            ConnectionState.Broken => "Broken",
            _ => "Неизвестно"
        };
        LbConnectionStatus.Content = "Статус подключения: " + status;
        BtnConnect.Content = state == ConnectionState.Open ? "Отключиться" : "Подключиться";
    }
    
    protected override void OnOpened(EventArgs e)
    {
        UpdateConnectionStatus();
        Database.Connection.StateChange += (_, _) =>
        {
            Dispatcher.UIThread.Invoke(UpdateConnectionStatus);
        };
        BtnConnect.Click += (_, _) =>
        {
            if (Database.Connection.State == ConnectionState.Open)
            {
                Database.Connection.CloseAsync();
            }
            else
            {
                Database.Connection.CloseAsync().ContinueWith(x => Database.Connection.OpenAsync());
            }
        };
        base.OnOpened(e);
    }
}