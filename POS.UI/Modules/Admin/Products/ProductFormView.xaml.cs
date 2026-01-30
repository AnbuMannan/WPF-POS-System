using POS.Shared.Models;
using POS.UI.Core.Services;
using POS.UI.Modules.Admin.Products.ViewModels;
using System;
using System.Windows;

namespace POS.UI.Modules.Admin.Products
{
    public partial class ProductFormView : Window
    {
        private readonly ProductFormViewModel _viewModel;

        public ProductFormView() : this(null) { }

        public ProductFormView(ProductDto editDto)
        {
            InitializeComponent();

            try
            {
                // Get ProductApiService from DI container
                if (App.ServiceProvider == null)
                    throw new InvalidOperationException("Application service provider not initialized.");

                var service = (ProductApiService)App.ServiceProvider.GetService(typeof(ProductApiService));
                if (service == null)
                    throw new InvalidOperationException("ProductApiService not registered in DI container.");

                _viewModel = new ProductFormViewModel(service, null, editDto);
                DataContext = _viewModel;

                // Subscribe to ViewModel events
                _viewModel.RequestClose += (s, e) => Close();
                _viewModel.RequestCloseWithResult += (s, result) =>
                {
                    DialogResult = result;
                    Close();
                };
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Initialization Error", $"Failed to initialize ProductFormView: {ex.Message}");
                Close();
            }
        }
    }
}
