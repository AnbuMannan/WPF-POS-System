using Dapper;
using Microsoft.AspNetCore.Connections;
using POS.AuthService.Infrastructure;
using POS.LicenseServer.Entities;

public class LicenseRepository
{
    private readonly DbConnectionFactory _factory;

    public LicenseRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public CentralLicense GetByKey(string key)
    {
        using var db = _factory.Create();
        return db.QueryFirstOrDefault<CentralLicense>(
            "SELECT * FROM CentralLicenses WHERE LicenseKey = @k",
            new { k = key });
    }

    public void Activate(string key, string machineId, int storeId)
    {
        using var db = _factory.Create();
        db.Execute(@"
          UPDATE CentralLicenses 
          SET IsActivated = 1,
              ActivatedOn = NOW(),
              MachineId = @m,
              StoreId = @s
          WHERE LicenseKey = @k",
          new { k = key, m = machineId, s = storeId });
    }
}
