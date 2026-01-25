using POS.UI.Modules.Admin.Products.Models;
using POS.UI.Modules.Admin.Products.ViewModels;
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

            _viewModel = new ProductFormViewModel(editDto);
            DataContext = _viewModel;

            // Subscribe to ViewModel events
            _viewModel.RequestClose += (s, e) => Close();
            _viewModel.RequestCloseWithResult += (s, result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}