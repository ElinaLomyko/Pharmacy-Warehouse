using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace Pharmacy.Data.Storage;

public class Database
{
    public static Database Instance = new Database();

    public MySqlConnection Connection = new MySqlConnection("server=localhost;uid=root;pwd=password;database=store");


    private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public bool IsOpened => Connection.State == ConnectionState.Open;
    
    public async Task OpenIfNeededAsync()
    {
        if (IsOpened)
        {
            return;
        }
        
        await _semaphore.WaitAsync();
        if (IsOpened)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await Connection.OpenAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        _semaphore.Release();
    }
}