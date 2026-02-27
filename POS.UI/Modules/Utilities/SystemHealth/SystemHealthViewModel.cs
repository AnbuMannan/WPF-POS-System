using Microsoft.Extensions.Configuration;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Media = System.Windows.Media;

namespace POS.UI.Modules.Utilities.SystemHealth
{
    public class SystemHealthViewModel : ViewModelBase
    {
        private readonly SystemHealthService _service;
        private readonly IConfiguration _config;

        public ObservableCollection<ServiceNode> Services { get; } = new();

        public ICommand CheckAllCommand { get; }

        public SystemHealthViewModel(SystemHealthService service, IConfiguration config)
        {
            _service = service;
            _config = config;

            // Init nodes from config
            Services.Clear();
            Services.Add(new ServiceNode { Name = "Core API", Url = _config["ApiSettings:BaseUrl"] ?? "" });
            Services.Add(new ServiceNode { Name = "Auth API", Url = _config["AuthApiBaseUrl"] ?? _config["AuthSettings:BaseUrl"] ?? "" });
            Services.Add(new ServiceNode { Name = "License API", Url = _config["AuthSettings:LicenseBaseUrl"] ?? _config["AuthApiBaseUrl"] ?? "" });

            CheckAllCommand = new RelayCommand(async () => await CheckAllAsync(), () => true);

            _ = CheckAllAsync();
        }

        private Media.Brush GetBrush(string key, Media.Brush fallback)
        {
            var res = System.Windows.Application.Current?.Resources[key] as Media.Brush;
            return res ?? fallback;
        }

        public async Task CheckAllAsync()
        {
            foreach (var node in Services)
            {
                node.IsChecking = true;
            }

            foreach (var node in Services)
            {
                var result = await _service.CheckServiceAsync(node.Name, node.Url);
                node.IsOnline = result.IsOnline;
                node.StatusColor = result.IsOnline
                    ? GetBrush("BrushSuccess", Media.Brushes.LimeGreen)
                    : GetBrush("BrushDanger", Media.Brushes.IndianRed);
                node.LatencyString = result.IsOnline ? $"Ping: {result.LatencyMs} ms" : "Offline";
                node.IsChecking = false;
            }
        }
    }
}
