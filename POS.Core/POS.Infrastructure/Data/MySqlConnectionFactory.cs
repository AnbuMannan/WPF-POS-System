using MySqlConnector;
using System.Data;

namespace POS.Infrastructure.Data;

public class MySqlConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Create()
        => new MySqlConnection(_connectionString);
}
