using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Components;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly LocalSettingsService _localSettings;
        private readonly StoreApiService _storeApi;

        private string _terminalId = string.Empty;
        private int _storeCode;
        private string _storeName = string.Empty;
        private string _selectedPrinter = string.Empty;
        private string _scannerPort = string.Empty;
        private List<string> _installedPrinters = new();

        public SettingsViewModel(LocalSettingsService localSettings, StoreApiService storeApi)
        {
            _localSettings = localSettings;
            _storeApi = storeApi;

            SaveSettingsCommand = new RelayCommand(SaveSettings);
            LoadSettings();
        }

        public int StoreCode
        {
            get => _storeCode;
            set { _storeCode = value; OnPropertyChanged(); }
        }

        public string StoreName
        {
            get => _storeName;
            set { _storeName = value; OnPropertyChanged(); }
        }

        public string TerminalId
        {
            get => _terminalId;
            set { _terminalId = value; OnPropertyChanged(); }
        }

        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set { _selectedPrinter = value; OnPropertyChanged(); }
        }

        public string ScannerPort
        {
            get => _scannerPort;
            set { _scannerPort = value; OnPropertyChanged(); }
        }

        public List<string> InstalledPrinters
        {
            get => _installedPrinters;
            set { _installedPrinters = value; OnPropertyChanged(); }
        }

        public ICommand SaveSettingsCommand { get; }

        private void LoadSettings()
        {
            var config = _localSettings.GetConfig();
            StoreCode = config.StoreCode;
            StoreName = config.StoreName;
            TerminalId = config.TerminalId;
            SelectedPrinter = config.ReceiptPrinterName;
            ScannerPort = config.BarcodeScannerPort;

            // Load printers
            try
            {
                InstalledPrinters = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
            }
            catch
            {
                InstalledPrinters = new List<string> { "Default Printer" };
            }
        }

        private void SaveSettings()
        {
            var config = _localSettings.GetConfig();
            config.TerminalId = TerminalId;
            config.ReceiptPrinterName = SelectedPrinter;
            config.BarcodeScannerPort = ScannerPort;

            _localSettings.SaveConfig(config);
            DialogService.Success("Settings Saved", "Configuration saved successfully. Please restart application to apply global hardware context.");
        }
    }
}