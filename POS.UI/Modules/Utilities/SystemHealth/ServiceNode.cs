using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS.UI.Modules.Utilities.SystemHealth
{
    public class ServiceNode : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _url = string.Empty;
        private bool _isOnline;
        private bool _isChecking;
        private string _latencyString = "-";
        private System.Windows.Media.Brush _statusColor = System.Windows.Media.Brushes.Gray;

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string Url { get => _url; set { _url = value; OnPropertyChanged(); } }
        public bool IsOnline { get => _isOnline; set { _isOnline = value; OnPropertyChanged(); } }
        public bool IsChecking { get => _isChecking; set { _isChecking = value; OnPropertyChanged(); } }
        public string LatencyString { get => _latencyString; set { _latencyString = value; OnPropertyChanged(); } }
        public System.Windows.Media.Brush StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
