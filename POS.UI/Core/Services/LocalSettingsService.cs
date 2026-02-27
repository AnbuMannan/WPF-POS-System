using System;
using System.IO;
using System.Text.Json;

namespace POS.UI.Core.Services
{
    public class LocalSettingsService
    {
        private readonly string _settingsFilePath;

        public LocalSettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var posFolderPath = Path.Combine(appDataPath, "MyPOS");
            if (!Directory.Exists(posFolderPath))
            {
                Directory.CreateDirectory(posFolderPath);
            }
            _settingsFilePath = Path.Combine(posFolderPath, "terminal_config.json");
        }

        public TerminalConfig GetConfig()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new TerminalConfig();
            }

            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                return JsonSerializer.Deserialize<TerminalConfig>(json) ?? new TerminalConfig();
            }
            catch
            {
                return new TerminalConfig();
            }
        }

        public void SaveConfig(TerminalConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }

        public int GetStoreCode() => GetConfig().StoreCode;
    }

    public class TerminalConfig
    {
        public int StoreCode { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
        public string ReceiptPrinterName { get; set; } = string.Empty;
        public string BarcodeScannerPort { get; set; } = string.Empty;
    }
}