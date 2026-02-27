using Microsoft.AspNetCore.DataProtection.KeyManagement;
using POS.AuthService;
using POS.AuthService.Entities;
using POS.AuthService.Repositories;
using POS.Shared.Models;
using System.Net.Http.Json;
using Dapper;
using Microsoft.EntityFrameworkCore;
using POS.AuthService.Infrastructure;

public class LicenseActivationService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly AuthRepository _repo;
    private readonly AuthDbContext _db;
    private readonly ILogger<LicenseActivationService> _logger;

    public LicenseActivationService(HttpClient http, IConfiguration config, AuthRepository repo, AuthDbContext db, ILogger<LicenseActivationService> logger)
    {
        _http = http;
        _config = config;
        _repo = repo;
        _db = db;
        _logger = logger;

        var baseUrl = _config["LicenseServerBaseUrl"] ?? "https://localhost:7143/";
        if (_http.BaseAddress == null)
        {
            _http.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<(bool success, string message, StoreDto? store)> ActivateOnlineAsync(string licenseKey)
    {
        var machineId = MachineHelper.GetMachineId();

        var request = new
        {
            LicenseKey = licenseKey,
            MachineId = machineId
        };

        try
        {
            var response = await _http.PostAsJsonAsync("api/license/activate", request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("License activation failed: {Status} {Body}", (int)response.StatusCode, body);
                return (false, body, null);
            }

            var result = await response.Content.ReadFromJsonAsync<LicenseActivationResponse>();
            if (result == null) return (false, "Invalid response from server", null);

            var signatureBytes = Convert.FromBase64String(result.Signature);

            // Verify HMAC signature against payload
            if (!LicenseCrypto.Verify(result.Payload, signatureBytes))
            {
                return (false, "Invalid license signature", null);
            }

            // Extract store details from payload
            var payload = System.Text.Json.JsonSerializer.Deserialize<LicensePayload>(result.Payload, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (payload == null)
            {
                return (false, "Invalid license payload", null);
            }

            await SaveLicenseLocallyAsync(
                licenseKey,
                machineId,
                payload.StoreId,
                payload.ExpiryDate,
                signatureBytes
            );

            // Upsert Store to Auth DB via EF Core
            var existing = await _db.Stores.FindAsync(payload.StoreId);
            if (existing != null)
            {
                existing.StoreName = payload.StoreName;
                existing.Address = payload.Address;
                existing.TaxId = payload.TaxId;
                existing.IsActive = true;
                _db.Stores.Update(existing);
            }
            else
            {
                _db.Stores.Add(new Store
                {
                    StoreCode = payload.StoreId,
                    StoreName = payload.StoreName,
                    Address = payload.Address,
                    TaxId = payload.TaxId,
                    IsActive = true
                });
            }
            await _db.SaveChangesAsync();

            return (true, "License activated and saved locally", new StoreDto 
            { 
                StoreCode = payload.StoreId, 
                StoreName = payload.StoreName,
                Address = payload.Address,
                TaxId = payload.TaxId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error contacting license server");
            return (false, "License server unreachable: " + ex.Message, null);
        }
    }

    private class LicensePayload
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? TaxId { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class LicenseActivationResponse
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
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
