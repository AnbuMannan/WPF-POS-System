using POS.UI.Core.Services;
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


            // 🔥 Important – bind your existing CategoryViewModel
            DataContext = new CategoryViewModel(new CategoryApiService(App.ApiClient));
        }
    }
}