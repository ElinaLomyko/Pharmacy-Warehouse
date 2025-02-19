using System;
using System.Data;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Pharmacy.Data.Storage;

namespace Pharmacy;

public partial class MainWindow : Window
{
    public static INotificationManager NotificationManager;
    
    public MainWindow()
    {
        InitializeComponent();

        NotificationManager = new WindowNotificationManager(GetTopLevel(this));
        
        UpdateConnectionStatus();
        Database.Instance.ConnectionReadOnly.StateChange += (_, _) =>
        {
            Dispatcher.UIThread.Invoke(UpdateConnectionStatus);
        };
        BtnConnect.Click += (_, _) =>
        {
            if (Database.Instance.ConnectionReadOnly.State == ConnectionState.Open)
            {
                Database.Instance.ConnectionReadOnly.CloseAsync();
            }
            else
            {
                if (!Database.Instance.IsOpened)
                {
                    _ = Database.Instance.ConnectionReadOnly.OpenAsync();
                }
            }
        };

    }

    private void UpdateConnectionStatus()
    {
        var state = Database.Instance.ConnectionReadOnly.State;
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
}