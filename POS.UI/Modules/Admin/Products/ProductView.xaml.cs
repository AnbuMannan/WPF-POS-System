using POS.UI.Core.Services;
using POS.UI.Modules.Admin.Products.ViewModels;
using System;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Admin.Products
{
    /// <summary>
    /// Interaction logic for ProductView.xaml
    /// </summary>
    public partial class ProductView : UserControl
    {
        public ProductView()
        {
            InitializeComponent();
            
            try
            {
                // Get ProductApiService from DI container
                if (App.ServiceProvider != null)
                {
                    var service = (ProductApiService)App.ServiceProvider.GetService(typeof(ProductApiService));
                    var viewModel = new ProductViewModel(service);
                    DataContext = viewModel;
                }
                else
                {
                    throw new InvalidOperationException("Application service provider not initialized.");
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Initialization Error", $"Failed to initialize ProductView: {ex.Message}");
            }
        }
    }
}
