using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using POS.UI.Modules.Admin.Products;
using POS.UI.Modules.Admin.Categories;
using POS.UI.Modules.Admin.Brands;
using POS.UI.Modules.Admin.TaxProfiles;

namespace POS.UI
{
    public partial class MainWindow : Window
    {
        private bool _isSidebarCollapsed = false;
        private DispatcherTimer _clockTimer;
        private List<ToggleButton> _allMenuButtons = new List<ToggleButton>();
        private ToggleButton _previousSelectedMenu = null;

        // Sidebar width constants
        private const double SIDEBAR_EXPANDED_WIDTH = 260;
        private const double SIDEBAR_COLLAPSED_WIDTH = 70;
        private const int ANIMATION_DURATION_MS = 300;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty IsSidebarOpenProperty =
            DependencyProperty.Register("IsSidebarOpen", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        public bool IsSidebarOpen
        {
            get { return (bool)GetValue(IsSidebarOpenProperty); }
            set { SetValue(IsSidebarOpenProperty, value); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize all menu buttons list
            _allMenuButtons = new List<ToggleButton>
            {
                BtnSales,
                BtnProducts,
                BtnCustomers,
                BtnSuppliers,
                BtnInventory,
                BtnPayments,
                BtnReports,
                BtnUsers,
                BtnOrganization,
                BtnFinance,
                BtnSettings,
                BtnUtilities,
                BtnSession
            };

            // Start clock
            //StartClock();
        }

        // ================= LIVE CLOCK =================
        //private void StartClock()
        //{
        //    _clockTimer = new DispatcherTimer();
        //    _clockTimer.Interval = TimeSpan.FromSeconds(1);
        //    _clockTimer.Tick += (s, e) =>
        //    {
        //        CurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
        //    };
        //    _clockTimer.Start();

        //    // Set initial time
        //    CurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
        //}

        // ================= SIDEBAR TOGGLE WITH ANIMATION =================
        private void SidebarToggle_Click(object sender, RoutedEventArgs e)
        {
            AnimateSidebarToggle();
        }

        private void AnimateSidebarToggle()
        {
            // Create animation for width change
            DoubleAnimation widthAnimation = new DoubleAnimation();
            widthAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ANIMATION_DURATION_MS));

            // Use exponential easing for smooth animation
            ExponentialEase easeFunc = new ExponentialEase();
            easeFunc.EasingMode = EasingMode.EaseInOut;
            widthAnimation.EasingFunction = easeFunc;

            if (_isSidebarCollapsed)
            {
                // Expand sidebar
                widthAnimation.From = SIDEBAR_COLLAPSED_WIDTH;
                widthAnimation.To = SIDEBAR_EXPANDED_WIDTH;
                _isSidebarCollapsed = false;

                // Show sidebar title text
                ShowSidebarText();
                IsSidebarOpen = true;
            }
            else
            {
                // Collapse sidebar
                widthAnimation.From = SIDEBAR_EXPANDED_WIDTH;
                widthAnimation.To = SIDEBAR_COLLAPSED_WIDTH;
                _isSidebarCollapsed = true;

                // Hide sidebar title text
                HideSidebarText();
                IsSidebarOpen = false;
            }

            // Apply animation to sidebar column
            SidebarContainer.BeginAnimation(Border.WidthProperty, widthAnimation);
        }

        private void ShowSidebarText()
        {
            // Text visibility is now handled by data binding to IsSidebarOpen
        }

        private void HideSidebarText()
        {
            // Text visibility is now handled by data binding to IsSidebarOpen
        }

        // ================= UNIFIED MENU CLICK HANDLER =================
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton clickedButton)
                return;

            // Clear all previous selections
            ClearAllMenuChecks();

            // Set the clicked button as checked
            clickedButton.IsChecked = true;
            _previousSelectedMenu = clickedButton;

            // Update header title based on selected menu
            UpdateHeaderTitle(clickedButton.Content?.ToString() ?? string.Empty);

            // Handle special menus (like Sales with submenu)
            if (clickedButton.Name.StartsWith("Btn"))
            {
                string popupName = string.Concat("Popup", clickedButton.Name.AsSpan(3));
                object popupObj = this.FindName(popupName);
                if (popupObj is Popup popup)
                {
                    popup.IsOpen = true;
                }
            }
        }

        private void ClearAllMenuChecks()
        {
            // Uncheck all menu buttons
            foreach (var button in _allMenuButtons)
            {
                button.IsChecked = false;
            }
        }

        private void UpdateHeaderTitle(string menuName)
        {
            // Update the header title based on selected menu
            HeaderTitle.Text = menuName;
        }

        // ================= SALES MENU POPUP =================
        private CustomPopupPlacement[] PopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            // Position popup to the right of sidebar
            double sidebarWidth = SidebarContainer.ActualWidth;
            var point = new Point(sidebarWidth, 0);

            return new[]
            {
                new CustomPopupPlacement(point, PopupPrimaryAxis.Vertical)
            };
        }

        // ================= SUBMENU NAVIGATION =================
        private void SubMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // Close the popup manually as we are navigating
                PopupProducts.IsOpen = false;

                switch (btn.Name)
                {
                    case "BtnProductList":
                        MainContent.Content = new ProductView();
                        HeaderTitle.Text = "Products List";
                        break;
                    case "BtnCategories":
                        MainContent.Content = new CategoryView();
                        HeaderTitle.Text = "Categories";
                        break;
                    case "BtnBrands":
                        MainContent.Content = new BrandView();
                        HeaderTitle.Text = "Brands";
                        break;
                    case "BtnTaxProfiles":
                        MainContent.Content = new TaxProfileView();
                        HeaderTitle.Text = "Tax Profiles";
                        break;
                }
            }
        }

        // ================= LOGOUT =================
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Show confirmation dialog
            var result = POS.UI.Components.DialogService.Confirm("Confirm Logout", "Are you sure you want to logout?");

            if (result == MessageBoxResult.Yes)
            {
                _clockTimer?.Stop();
                Application.Current.Shutdown();
            }
        }
    }
}
