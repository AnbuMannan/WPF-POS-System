using MySqlConnector;
using System.Data;

namespace POS.Infrastructure.Data;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Create()
        => new MySqlConnection(_connectionString);
}
