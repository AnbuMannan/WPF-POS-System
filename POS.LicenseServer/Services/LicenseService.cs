namespace POS.LicenseServer.Services
{
    public class LicenseService
    {
        private readonly LicenseRepository _repo;

        public LicenseService(LicenseRepository repo)
        {
            _repo = repo;
        }

        public (bool success, byte[] signature, DateTime expiryDate, string message) ActivateSigned(string key, string machineId, int storeId)
        {
            var lic = _repo.GetByKey(key);

            if (lic == null)
                return (false, null, default, "Invalid license key");

            if (lic.IsRevoked)
                return (false, null, default, "License revoked");

            if (lic.ExpiryDate < DateTime.UtcNow)
                return (false, null, default, "License expired");

            if (lic.IsActivated)
                return (false, null, default, "License already activated");

            var payload = $"{key}|{machineId}|{storeId}|{lic.ExpiryDate:yyyy-MM-dd}";
            var signature = LicenseCrypto.Sign(payload);

            _repo.Activate(key, machineId, storeId);

            return (true, signature, lic.ExpiryDate, "License activated successfully");
        }


    }

}
