using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;
using System.Data.Common;

namespace POS.AuthService.Infrastructure
{
    public class DbConnectionFactory
    {
        private readonly IConfiguration _config;

        public DbConnectionFactory(IConfiguration config)
        {
            _config = config;
        }


        public DbConnection Create()
        {
            return new MySqlConnection(_config.GetConnectionString("MySql"));
        }


}
}
