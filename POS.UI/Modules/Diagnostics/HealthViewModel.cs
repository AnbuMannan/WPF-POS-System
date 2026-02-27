using Microsoft.Extensions.Configuration;
using POS.UI.Core.MVVM;
using System;
using System.Net.Http;

namespace POS.UI.Modules.Diagnostics
{
    public class HealthViewModel : ViewModelBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;

        private string _apiBaseUrl = string.Empty;
        private string _authApiBaseUrl = string.Empty;
        private string _licenseBaseUrl = string.Empty;
        private string _defaultApiClientBase = string.Empty;

        public HealthViewModel(IConfiguration config, IHttpClientFactory httpFactory)
        {
            _config = config;
            _httpFactory = httpFactory;
            Load();
        }

        public string ApiBaseUrl
        {
            get => _apiBaseUrl;
            set { _apiBaseUrl = value; OnPropertyChanged(); }
        }

        public string AuthApiBaseUrl
        {
            get => _authApiBaseUrl;
            set { _authApiBaseUrl = value; OnPropertyChanged(); }
        }

        public string LicenseBaseUrl
        {
            get => _licenseBaseUrl;
            set { _licenseBaseUrl = value; OnPropertyChanged(); }
        }

        public string DefaultApiClientBase
        {
            get => _defaultApiClientBase;
            set { _defaultApiClientBase = value; OnPropertyChanged(); }
        }

        private void Load()
        {
            ApiBaseUrl = _config["ApiSettings:BaseUrl"] ?? string.Empty;
            AuthApiBaseUrl = _config["AuthApiBaseUrl"] ?? _config["AuthSettings:BaseUrl"] ?? string.Empty;
            LicenseBaseUrl = _config["AuthSettings:LicenseBaseUrl"] ?? AuthApiBaseUrl;

            try
            {
                var client = _httpFactory.CreateClient("DefaultApi");
                DefaultApiClientBase = client.BaseAddress?.ToString() ?? string.Empty;
            }
            catch
            {
                DefaultApiClientBase = string.Empty;
            }
        }
    }
}
