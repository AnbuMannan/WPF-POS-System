using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Size = System.Windows.Size;
using Point = System.Windows.Point;
using Microsoft.Extensions.Configuration;
using POS.UI.Core;
using POS.UI.Modules.Admin.Products;
using POS.UI.Modules.Admin.Categories;
using POS.UI.Modules.Admin.Brands;
using POS.UI.Modules.Admin.TaxProfiles;
using POS.UI.Modules.Admin.Uom;
using POS.UI.Modules.Admin.Customers;
using POS.UI.Modules.Billing.BillingScreen;
using POS.UI.Modules.Billing.ReturnDialog;
using POS.UI.Modules.Admin.Products.BarcodeLabel;
using POS.UI.Modules.Reports.EODReport;
using POS.UI.Modules.Reports.AuditLog;
using POS.UI.Core.Navigation;
using POS.UI.Core.Services;

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

            StartClock();
            UpdateStatusBar();
            NavigateToBillingView();
        }

        private void StartClock()
        {
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) =>
            {
                StatusBarDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            _clockTimer.Start();
            StatusBarDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void UpdateStatusBar()
        {
            StatusBarUser.Text = string.IsNullOrEmpty(AppState.CurrentUserName) ? "User: --" : $"User: {AppState.CurrentUserName}";
            StatusBarOnline.Text = AppState.IsOnline ? "Online" : "Offline";
            var config = App.ServiceProvider?.GetService(typeof(IConfiguration)) as IConfiguration;
            var version = config?["Application:Version"] ?? "1.0.0";
            StatusBarVersion.Text = $"v{version}";
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
        private void SubMenuSales_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                PopupSales.IsOpen = false;

                if (btn.Name == "BtnBilling")
                {
                    var view = ViewResolver.Resolve("BillingView");
                    MainContent.Content = view;
                    HeaderTitle.Text = "Billing / New Sale";
                }
                else if (btn.Name == "BtnReprintInvoice")
                {
                    ReprintLastReceipt();
                }
                else if (btn.Name == "BtnReturnsExchanges")
                {
                    OpenReturnDialog();
                }
                else if (btn.Name == "BtnDayEnd")
                {
                    MainContent.Content = new EODReportView();
                    HeaderTitle.Text = "Day End / EOD Report";
                }
                else if (btn.Name == "BtnQuickSale")
                {
                    OpenQuickSaleDialog();
                }
            }
        }

        private void OpenQuickSaleDialog()
        {
            var billingApi = App.ServiceProvider?.GetService(typeof(BillingApiService)) as BillingApiService;
            var productApi = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;
            var taxProfileApi = App.ServiceProvider?.GetService(typeof(TaxProfileApiService)) as TaxProfileApiService;
            var uomApi = App.ServiceProvider?.GetService(typeof(UomApiService)) as UomApiService;
            if (billingApi == null || productApi == null || taxProfileApi == null || uomApi == null)
            {
                POS.UI.Components.DialogService.Warning("Quick Sale", "Services not available.");
                return;
            }
            var dialog = new POS.UI.Modules.Billing.QuickSale.QuickSaleDialog(billingApi, productApi, taxProfileApi, uomApi) { Owner = this };
            dialog.ShowDialog();
        }

        private void OpenReturnDialog()
        {
            var returnApi = App.ServiceProvider?.GetService(typeof(ReturnApiService)) as ReturnApiService;
            var productApi = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;
            var taxProfileApi = App.ServiceProvider?.GetService(typeof(TaxProfileApiService)) as TaxProfileApiService;
            var uomApi = App.ServiceProvider?.GetService(typeof(UomApiService)) as UomApiService;
            if (returnApi == null || productApi == null || taxProfileApi == null || uomApi == null)
            {
                POS.UI.Components.DialogService.Warning("Returns", "Services not available.");
                return;
            }
            var dialog = new ReturnDialog(returnApi, productApi, taxProfileApi, uomApi) { Owner = this };
            dialog.ShowDialog();
        }

        private async void ReprintLastReceipt()
        {
            var printSettings = App.ServiceProvider?.GetService(typeof(POS.UI.Core.Services.IPrintSettingsService)) as POS.UI.Core.Services.IPrintSettingsService;
            var billingApi = App.ServiceProvider?.GetService(typeof(POS.UI.Core.Services.BillingApiService)) as POS.UI.Core.Services.BillingApiService;
            var printService = App.ServiceProvider?.GetService(typeof(POS.UI.Core.Services.IPrintService)) as POS.UI.Core.Services.IPrintService;
            var emailService = App.ServiceProvider?.GetService(typeof(POS.UI.Core.Services.IEmailReceiptService)) as POS.UI.Core.Services.IEmailReceiptService;
            var saleId = printSettings?.GetLastPrintedSaleId();
            if (!saleId.HasValue || saleId.Value <= 0)
            {
                POS.UI.Components.DialogService.Info("Reprint", "No receipt has been printed yet. Complete a sale first.");
                return;
            }
            if (billingApi == null || printService == null)
            {
                POS.UI.Components.DialogService.Warning("Reprint", "Services not available.");
                return;
            }
            var receipt = await billingApi.GetReceiptBySaleIdAsync(saleId.Value);
            if (receipt == null)
            {
                POS.UI.Components.DialogService.Warning("Reprint", "Could not load receipt for the last sale.");
                return;
            }
            var dialog = new POS.UI.Modules.Billing.ReceiptPreview.PrintPreviewDialog(receipt, printService, emailService) { Owner = this };
            dialog.ShowDialog();
        }

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
                    case "BtnUom":
                        MainContent.Content = new UomView();
                        HeaderTitle.Text = "Units of Measurement (UoM)";
                        break;
                    case "BtnBarcodeManagement":
                        OpenBarcodeLabelDialog();
                        break;
                }
            }
        }

        private void OpenBarcodeLabelDialog()
        {
            var productApi = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;
            var printService = App.ServiceProvider?.GetService(typeof(IPrintService)) as IPrintService;
            if (productApi == null || printService == null)
            {
                POS.UI.Components.DialogService.Warning("Barcode Labels", "Services not available.");
                return;
            }
            var dialog = new BarcodeLabelDialog(productApi, printService) { Owner = this };
            dialog.ShowDialog();
        }

        private void SubMenuCustomers_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                PopupCustomers.IsOpen = false;
                switch (btn.Name)
                {
                    case "BtnCustomerList":
                        MainContent.Content = new CustomerView();
                        HeaderTitle.Text = "Customers";
                        break;
                }
            }
        }

        // ================= REPORTS SUBMENU =================
        private void SubMenuReports_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            PopupReports.IsOpen = false;
            if (btn.Name == "BtnAuditLogs")
            {
                MainContent.Content = new AuditLogView();
                HeaderTitle.Text = "Audit Log";
            }
        }

        // ================= SETTINGS SUBMENU =================
        private void SubMenuSettings_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            PopupSettings.IsOpen = false;
            if (btn.Name == "BtnPrintSettings")
            {
                var printSettings = App.ServiceProvider?.GetService(typeof(POS.UI.Core.Services.IPrintSettingsService)) as POS.UI.Core.Services.IPrintSettingsService;
                if (printSettings == null) return;
                var printService = App.ServiceProvider?.GetService(typeof(POS.UI.Core.Services.IPrintService)) as POS.UI.Core.Services.IPrintService;
                var dialog = new POS.UI.Modules.Settings.PrintSettingsDialog(printSettings, printService) { Owner = this };
                dialog.ShowDialog();
            }
        }

        // ================= LOGOUT =================
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = POS.UI.Components.DialogService.Confirm("Confirm Logout", "Are you sure you want to logout?");
            if (result == MessageBoxResult.Yes)
            {
                AppState.ClearUser();
                _clockTimer?.Stop();
                Application.Current.Shutdown();
            }
        }

        private void NavigateToBillingView()
        {
            MainContent.Content = ViewResolver.Resolve("BillingView");
            HeaderTitle.Text = "Billing / New Sale";
        }

        // ================= MENU BAR HANDLERS (File, Sales, Reports, Settings, Help) =================
        private void MenuFile_NewSale(object sender, RoutedEventArgs e) => NavigateToBillingView();
        private void MenuFile_Exit(object sender, RoutedEventArgs e) => Logout_Click(sender, e);

        private void MenuSales_Billing(object sender, RoutedEventArgs e) => NavigateToBillingView();
        private void MenuSales_QuickSale(object sender, RoutedEventArgs e) => OpenQuickSaleDialog();
        private void MenuSales_Returns(object sender, RoutedEventArgs e) => OpenReturnDialog();
        private void MenuSales_DayEnd(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new EODReportView();
            HeaderTitle.Text = "Day End / EOD Report";
        }

        private void MenuReports_AuditLogs(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new AuditLogView();
            HeaderTitle.Text = "Audit Log";
        }

        private void MenuSettings_Print(object sender, RoutedEventArgs e)
        {
            var printSettings = App.ServiceProvider?.GetService(typeof(IPrintSettingsService)) as IPrintSettingsService;
            if (printSettings == null) return;
            var printService = App.ServiceProvider?.GetService(typeof(IPrintService)) as IPrintService;
            var dialog = new POS.UI.Modules.Settings.PrintSettingsDialog(printSettings, printService) { Owner = this };
            dialog.ShowDialog();
        }
    }
}
