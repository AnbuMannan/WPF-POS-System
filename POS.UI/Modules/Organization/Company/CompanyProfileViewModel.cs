using Microsoft.Win32;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POS.UI.Modules.Organization.Company
{
    public class CompanyProfileViewModel : ViewModelBase
    {
        private readonly CompanyProfileApiService _service;

        // Properties
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        private string _city = string.Empty;
        public string City
        {
            get => _city;
            set { _city = value; OnPropertyChanged(); }
        }

        private string _state = string.Empty;
        public string State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        private string _postalCode = string.Empty;
        public string PostalCode
        {
            get => _postalCode;
            set { _postalCode = value; OnPropertyChanged(); }
        }

        private string _country = string.Empty;
        public string Country
        {
            get => _country;
            set { _country = value; OnPropertyChanged(); }
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        private string _mobile = string.Empty;
        public string Mobile
        {
            get => _mobile;
            set { _mobile = value; OnPropertyChanged(); }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _website = string.Empty;
        public string Website
        {
            get => _website;
            set { _website = value; OnPropertyChanged(); }
        }

        private string _gstNumber = string.Empty;
        public string GstNumber
        {
            get => _gstNumber;
            set { _gstNumber = value; OnPropertyChanged(); }
        }

        private string _panNumber = string.Empty;
        public string PanNumber
        {
            get => _panNumber;
            set { _panNumber = value; OnPropertyChanged(); }
        }

        private string _logoUrl = string.Empty;
        public string LogoUrl
        {
            get => _logoUrl;
            set { _logoUrl = value; OnPropertyChanged(); }
        }

        private string _currencySymbol = "₹";
        public string CurrencySymbol
        {
            get => _currencySymbol;
            set { _currencySymbol = value; OnPropertyChanged(); }
        }

        private string _currencyCode = "INR";
        public string CurrencyCode
        {
            get => _currencyCode;
            set { _currencyCode = value; OnPropertyChanged(); }
        }

        private string _receiptHeader = string.Empty;
        public string ReceiptHeader
        {
            get => _receiptHeader;
            set { _receiptHeader = value; OnPropertyChanged(); }
        }

        private string _receiptFooter = string.Empty;
        public string ReceiptFooter
        {
            get => _receiptFooter;
            set { _receiptFooter = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand UploadLogoCommand { get; }

        public CompanyProfileViewModel(CompanyProfileApiService service)
        {
            _service = service;

            LoadCommand = new RelayCommand(async () => await LoadAsync());
            SaveCommand = new RelayCommand(async () => await SaveAsync());
            UploadLogoCommand = new RelayCommand(async () => await UploadLogoAsync());

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var profile = await _service.GetAsync();
                if (profile != null)
                {
                    Name = profile.Name;
                    Address = profile.Address ?? string.Empty;
                    City = profile.City ?? string.Empty;
                    State = profile.State ?? string.Empty;
                    PostalCode = profile.PostalCode ?? string.Empty;
                    Country = profile.Country ?? string.Empty;
                    Phone = profile.Phone ?? string.Empty;
                    Mobile = profile.Mobile ?? string.Empty;
                    Email = profile.Email ?? string.Empty;
                    Website = profile.Website ?? string.Empty;
                    GstNumber = profile.GstNumber ?? string.Empty;
                    PanNumber = profile.PanNumber ?? string.Empty;
                    LogoUrl = profile.LogoUrl ?? string.Empty;
                    CurrencySymbol = profile.CurrencySymbol ?? "₹";
                    CurrencyCode = profile.CurrencyCode ?? "INR";
                    ReceiptHeader = profile.ReceiptHeader ?? string.Empty;
                    ReceiptFooter = profile.ReceiptFooter ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Components.DialogService.Error("Failed to load company profile", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                Components.DialogService.Warning("Validation", "Company name is required");
                return;
            }

            try
            {
                IsLoading = true;

                var dto = new UpdateCompanyProfileDto
                {
                    Name = Name,
                    Address = Address,
                    City = City,
                    State = State,
                    PostalCode = PostalCode,
                    Country = Country,
                    Phone = Phone,
                    Mobile = Mobile,
                    Email = Email,
                    Website = Website,
                    GstNumber = GstNumber,
                    PanNumber = PanNumber,
                    LogoUrl = LogoUrl,
                    CurrencySymbol = CurrencySymbol,
                    CurrencyCode = CurrencyCode,
                    ReceiptHeader = ReceiptHeader,
                    ReceiptFooter = ReceiptFooter
                };

                var (success, message, profile) = await _service.SaveAsync(dto);

                if (success)
                {
                    Components.DialogService.Info("Success", message);
                    
                    // Update AppState with company info
                    if (profile != null)
                    {
                        Core.AppState.CompanyName = profile.Name;
                        Core.AppState.CurrencySymbol = profile.CurrencySymbol ?? "₹";
                    }
                }
                else
                {
                    Components.DialogService.Error("Failed", message);
                }
            }
            catch (Exception ex)
            {
                Components.DialogService.Error("Failed to save company profile", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UploadLogoAsync()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Company Logo",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    var result = await _service.UploadLogoAsync(dialog.FileName);
                    
                    if (result.Success && !string.IsNullOrEmpty(result.LogoUrl))
                    {
                        LogoUrl = result.LogoUrl;
                        Components.DialogService.Info("Success", result.Message);
                    }
                    else
                    {
                        Components.DialogService.Error("Failed", result.Message);
                    }
                }
                catch (Exception ex)
                {
                    Components.DialogService.Error("Failed to upload logo", ex.Message);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}
