using POS.UI.Core.Services;
using System;
using System.Windows.Controls;


namespace POS.UI.Modules.Admin.Categories
{
    /// <summary>
    /// Interaction logic for CategoryView.xaml
    /// </summary>
    public partial class CategoryView : UserControl
    {
        public CategoryView()
        {
            InitializeComponent();

            try
            {
                // Get CategoryApiService from DI container
                if (App.ServiceProvider != null)
                {
                    var service = (CategoryApiService)App.ServiceProvider.GetService(typeof(CategoryApiService));
                    var viewModel = new CategoryViewModel(service);
                    DataContext = viewModel;
                }
                else
                {
                    throw new InvalidOperationException("Application service provider not initialized.");
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Initialization Error", $"Failed to initialize CategoryView: {ex.Message}");
            }
        }
    }
}
