using Microsoft.AspNetCore.DataProtection.KeyManagement;
using POS.AuthService;
using POS.AuthService.Repositories;
using System.Net.Http.Json;

public class LicenseActivationService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly AuthRepository _repo;

    public LicenseActivationService(HttpClient http, IConfiguration config, AuthRepository repo)
    {
        _http = http;
        _config = config;
        _repo = repo;
    }

    public async Task<(bool success, string message)> ActivateOnlineAsync(string licenseKey, int storeId)
    {
        var machineId = MachineHelper.GetMachineId();

        var request = new
        {
            LicenseKey = licenseKey,
            MachineId = machineId,
            StoreId = storeId
        };

        var url = _config["LicenseServer:ActivateUrl"];

        try
        {
            if (_repo.GetLocalLicense() != null)
                return (true, "License already activated locally");

            var response = await _http.PostAsJsonAsync(url, request);

            if (!response.IsSuccessStatusCode)
                return (false, await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadFromJsonAsync<LicenseActivationResponse>();

            var signatureBytes = Convert.FromBase64String(result.Signature);

            await SaveLicenseLocallyAsync(
                licenseKey,
                machineId,
                storeId,
                result.ExpiryDate,
                signatureBytes
            );

            return (true, "License activated and saved locally");
        }
        catch (Exception ex)
        {
            return (false, "License server unreachable: " + ex.Message);
        }
    }

    public class LicenseActivationResponse
    {
        public string LicenseKey { get; set; }
        public string Signature { get; set; }
        public DateTime ExpiryDate { get; set; }
    }


    private async Task SaveLicenseLocallyAsync(
    string key,
    string machineId,
    int storeId,
    DateTime expiryDate,
    byte[] signatureBytes)
    {
        await _repo.SaveLocalLicense(
            key,
            machineId,
            storeId,
            expiryDate,
            signatureBytes
        );
    }
}
