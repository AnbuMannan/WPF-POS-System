namespace POS.LicenseServer.Services
{
    public class LicenseService
    {
        private readonly LicenseRepository _repo;

        public LicenseService(LicenseRepository repo)
        {
            _repo = repo;
        }

        public (bool success, string payload, byte[] signature, DateTime expiryDate, string message) ActivateSigned(string key, string machineId)
        {
            var lic = _repo.GetByKey(key);

            if (lic == null)
                return (false, null, null, default, "Invalid license key");

            if (lic.IsRevoked)
                return (false, null, null, default, "License revoked");

            if (lic.ExpiryDate < DateTime.Now)
                return (false, null, null, default, "License expired");

            // Idempotency: allow safe retry from the same machine
            if (lic.IsActivated)
            {
                if (!string.IsNullOrEmpty(lic.MachineId) && string.Equals(lic.MachineId, machineId, StringComparison.Ordinal))
                {
                    var existingPayload = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        LicenseKey = key,
                        MachineId = machineId,
                        StoreId = lic.StoreId,
                        StoreName = lic.StoreName,
                        Address = lic.Address,
                        TaxId = lic.TaxId,
                        ExpiryDate = lic.ExpiryDate
                    });
                    var existingSignature = LicenseCrypto.Sign(existingPayload);
                    return (true, existingPayload, existingSignature, lic.ExpiryDate, "License already activated for this machine");
                }
                return (false, null, null, default, "License is already activated on a different machine.");
            }

            var payloadJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                LicenseKey = key,
                MachineId = machineId,
                StoreId = lic.StoreId,
                StoreName = lic.StoreName,
                Address = lic.Address,
                TaxId = lic.TaxId,
                ExpiryDate = lic.ExpiryDate
            });
            var signature = LicenseCrypto.Sign(payloadJson);

            _repo.Activate(key, machineId, lic.StoreId);

            return (true, payloadJson, signature, lic.ExpiryDate, "License activated successfully");
        }


    }

}
