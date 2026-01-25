using POS.UI.Core.Navigation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using POS.UI.Modules.Admin.Products;
using POS.UI.Modules.Admin.Brands;
using POS.UI.Modules.Admin.Categories;
using POS.UI.Modules.Billing.BillingScreen;
using POS.UI.Modules.Inventory.StockView;
using POS.UI.Modules.Billing.BillingScreen;

namespace POS.UI
{
    public partial class MainWindow : Window
    {
        private List<MenuItemModel> _menuItems;

        public MainWindow()
        {
            InitializeComponent();
            LoadMenuFromJson();
            LoadTopMenu(_menuItems);
        }

        // ================= Load Menu JSON =================
        private void LoadMenuFromJson()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "menu.json");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("menu.json not found!");
                return;
            }

            var json = File.ReadAllText(filePath);
            _menuItems = JsonSerializer.Deserialize<List<MenuItemModel>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        // ================= Build Top Menu =================
        private void LoadTopMenu(IEnumerable<MenuItemModel> menuItems)
        {
            TopMenu.Items.Clear();

            foreach (var item in menuItems)
            {
                TopMenu.Items.Add(CreateMenuItem(item));
            }
        }

        private MenuItem CreateMenuItem(MenuItemModel model)
        {
            var menuItem = new MenuItem
            {
                Header = model.Header,
                FontWeight = FontWeights.SemiBold
            };

            if (model.Children != null && model.Children.Any())
            {
                // TOP LEVEL MENU
                menuItem.Style = (Style)FindResource("PosTopMenuItemStyle");

                foreach (var child in model.Children)
                {
                    menuItem.Items.Add(CreateMenuItem(child));
                }
            }
            else
            {
                // SUB MENU ITEM
                menuItem.Style = (Style)FindResource("PosSubMenuItemStyle");
                menuItem.Tag = model.View;
                menuItem.Click += MenuItem_Click;
            }

            return menuItem;
        }



        // ================= Menu Click Navigation =================
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem clickedItem)
                return;

            var viewName = clickedItem.Tag?.ToString();

            if (string.IsNullOrWhiteSpace(viewName))
                return;

            NavigateToView(viewName);
        }

        // ================= View Navigation =================
        private void NavigateToView(string viewName)
        {
            switch (viewName)
            {
                case "ProductView":
                    MainContent.Content = new ProductView();
                    break;

                case "BrandsView":
                    MainContent.Content = new BrandView();
                    break;

                case "CategoryView":
                    MainContent.Content = new CategoryView();
                    break;

                case "InvoiceView":
                    MainContent.Content = new BillingView();
                    break;

                case "StockView":
                    MainContent.Content = new StockView();
                    break;

                default:
                    MessageBox.Show($"View not registered: {viewName}");
                    break;
            }
        }

    }
}
