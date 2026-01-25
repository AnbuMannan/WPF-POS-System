using System;
using System.Net.Http;
using System.Windows;

namespace POS.UI
{
    public partial class App : Application
    {
        public static HttpClient ApiClient { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ApiClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7285/") // 🔁 change to your API URL
            };
        }
    }
}
