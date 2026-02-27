using Dapper;
using Microsoft.AspNetCore.DataProtection;
using POS.AuthService.Entities;
using POS.AuthService.Infrastructure;
using System.Reflection.PortableExecutable;

namespace POS.AuthService.Repositories
{
    public class AuthRepository
    {
        private readonly DbConnectionFactory _factory;
        public class LocalLicense
        {
            public string LicenseKey { get; set; }
            public string MachineId { get; set; }
            public int StoreId { get; set; }
            public DateTime ExpiryDate { get; set; }
            public byte[] LicenseSignature { get; set; }
        }

        public AuthRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public System.Data.IDbConnection GetConnection() => _factory.Create();

        public User GetUserByUsername(string username)
        {
            using var db = _factory.Create();
            return db.QueryFirstOrDefault<User>(
                "SELECT * FROM Users WHERE Username = @u AND IsActive = 1",
                new { u = username });
        }

        public List<string> GetPermissionsByRole(int roleId)
        {
            using var db = _factory.Create();
            return db.Query<string>(
                @"SELECT p.Code FROM RolePermissions rp
                  JOIN Permissions p ON rp.PermissionId = p.Id
                  WHERE rp.RoleId = @r", new { r = roleId }).ToList();
        }

        public Role GetRole(int roleId)
        {
            using var db = _factory.Create();
            return db.QueryFirstOrDefault<Role>(
                "SELECT * FROM Roles WHERE Id = @id", new { id = roleId });
        }
        public bool IsLicenseValid(string machineId)
        {
            using var db = _factory.Create();
            return db.ExecuteScalar<int>(
              @"SELECT COUNT(*) FROM License 
        WHERE IsValid = 1 AND MachineId = @m",
                new { m = machineId }) > 0;
        }

        public bool IsLicenseValidForMachine(string machineId)
        {
            using var db = _factory.Create();
            return db.ExecuteScalar<int>(
              @"SELECT COUNT(*) FROM License l
        JOIN Devices d ON l.DeviceId = d.Id
        WHERE l.IsValid = 1 
        AND d.MachineId = @m
        AND l.ExpiryDate >= CURDATE()",
                new { m = machineId }) > 0;
        }

        public async Task SaveLocalLicense(
        string key,
        string machineId,
        int storeId,
        DateTime expiry,
        byte[] rawSignature)
        {
            using var db = _factory.Create();
            await db.OpenAsync();   // 🔥 IMPORTANT FIX

            using var tx = db.BeginTransaction();

            try
            {
                var encryptedSignature = DataProtector.Encrypt(rawSignature);

                // Ensure device exists
                var deviceId = await db.ExecuteScalarAsync<int?>(@"
            SELECT Id FROM Devices WHERE MachineId = @m",
                    new { m = machineId }, tx);

                if (!deviceId.HasValue)
                {
                    await db.ExecuteAsync(@"
                INSERT INTO Devices (StoreId, MachineId, DeviceName, IsActive)
                VALUES (@s, @m, @n, 1)",
                        new { s = storeId, m = machineId, n = machineId }, tx);

                    deviceId = await db.ExecuteScalarAsync<int>(
                        "SELECT LAST_INSERT_ID()", transaction: tx);
                }

                // Save license
                await db.ExecuteAsync(@"
            INSERT INTO License 
            (LicenseKey, MachineId, StoreId, DeviceId, ActivatedOn, ExpiryDate, IsValid, LicenseSignature, SignatureAlgorithm)
            VALUES (@k, @m, @s, @d, NOW(), @e, 1, @sig, 'HMACSHA256')
            ON DUPLICATE KEY UPDATE
                LicenseKey = @k,
                StoreId = @s,
                DeviceId = @d,
                ActivatedOn = NOW(),
                ExpiryDate = @e,
                IsValid = 1,
                LicenseSignature = @sig,
                SignatureAlgorithm = 'HMACSHA256'
            ",
                    new
                    {
                        k = key,
                        m = machineId,
                        s = storeId,
                        d = deviceId.Value,
                        e = expiry,
                        sig = encryptedSignature
                    }, tx);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }





        public LocalLicense GetLocalLicense()
        {
            using var db = _factory.Create();
            return db.QueryFirstOrDefault<LocalLicense>(@"
        SELECT 
            LicenseKey,
            MachineId,
            StoreId,
            ExpiryDate,
            LicenseSignature
        FROM License
        LIMIT 1");
        }

        public int EnsureDevice(string machineId, int storeId)
        {
            using var db = _factory.Create();

            // Check existing device
            var existing = db.ExecuteScalar<int?>(
                "SELECT Id FROM Devices WHERE MachineId = @m",
                new { m = machineId });

            if (existing.HasValue)
                return existing.Value;

            // Insert new device
            db.Execute(@"
        INSERT INTO Devices (StoreId, MachineId, DeviceName, IsActive)
        VALUES (@s, @m, @n, 1)",
                new { s = storeId, m = machineId, n = machineId });

            // Fetch new Id
            return db.ExecuteScalar<int>("SELECT LAST_INSERT_ID();");
        }

        public void SaveRefreshToken(int userId, string refreshToken, DateTime expiryDate)
        {
            using var db = _factory.Create();
            db.Execute(
                @"UPDATE Users 
                  SET RefreshToken = @rt, RefreshTokenExpiryDate = @exp, LastLogin = NOW()
                  WHERE Id = @id",
                new { rt = refreshToken, exp = expiryDate, id = userId });
        }

        public bool ValidateRefreshToken(int userId, string refreshToken)
        {
            using var db = _factory.Create();
            var result = db.QueryFirstOrDefault<dynamic>(
                @"SELECT RefreshToken, RefreshTokenExpiryDate FROM Users 
                  WHERE Id = @id AND RefreshToken = @rt AND RefreshTokenExpiryDate > NOW()",
                new { id = userId, rt = refreshToken });

            return result != null;
        }
    }
}
