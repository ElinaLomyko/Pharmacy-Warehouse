using MySqlConnector;

namespace Pharmacy.Storage;

public class Database
{
    public static MySqlConnection Connection { get; set; }

    public static void SetupConnection()
    {
        Connection = new MySqlConnection("server=localhost;uid=root;pwd=password;database=store");
    }
}