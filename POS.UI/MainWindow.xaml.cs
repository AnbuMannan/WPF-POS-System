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
using POS.UI.Modules.Suppliers.SupplierList;
using POS.UI.Modules.Suppliers.PurchaseOrder;
using POS.UI.Modules.Suppliers.PurchaseEntry;
using POS.UI.Modules.Suppliers.PurchaseReturn;
using POS.UI.Modules.Suppliers.SupplierPayments;
using POS.UI.Modules.Suppliers.SupplierLedger;
using POS.UI.Modules.Inventory.StockAdjustment;
using POS.UI.Modules.Inventory.ItemLedger;
using POS.UI.Modules.Inventory.LabelPrinting;
using POS.UI.Modules.Billing.BillingScreen;
using POS.UI.Modules.Billing.ReturnDialog;
using POS.UI.Modules.Admin.Products.BarcodeLabel;
using POS.UI.Modules.Reports.EODReport;
using POS.UI.Modules.Reports.AuditLog;
using POS.UI.Modules.Reports.Sales;
using POS.UI.Modules.Reports.Inventory;
using POS.UI.Modules.Reports.GSTReports;
using POS.UI.Modules.Reports.Finance;
using POS.UI.Modules.Users.UserList;
using POS.UI.Modules.Users.Roles;
using POS.UI.Modules.Payments.CashManagement;
using POS.UI.Modules.Organization.Company;
using POS.UI.Core.Navigation;
using POS.UI.Core.Services;
using POS.UI.Modules.Sales.Returns;
using POS.UI.Modules.Sales.Quotations;
using POS.UI.Modules.Customers.Outstanding;
using POS.Shared.Models;

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
            _allMenuButtons = new List<ToggleButton>
            {
                BtnDashboard,
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
            NavigateToDashboardView();
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

            if (clickedButton == BtnDashboard)
            {
                NavigateToDashboardView();
                return;
            }

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
                else if (btn.Name == "BtnSalesReturn")
                {
                    ShowSalesReturnList();
                }
                else if (btn.Name == "BtnQuotations")
                {
                    ShowQuotationList();
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
                    case "BtnCustomerOutstanding":
                        ShowCustomerOutstanding();
                        break;
                    case "BtnLoyaltySettings":
                        MainContent.Content = new POS.UI.Modules.Customers.Loyalty.LoyaltySettingsView();
                        HeaderTitle.Text = "Loyalty Program";
                        break;
                }
            }
        }

        private void SubMenuSuppliers_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                PopupSuppliers.IsOpen = false;
                switch (btn.Name)
                {
                    case "BtnSupplierList":
                        var supplierApiService = App.ServiceProvider?.GetService(typeof(SupplierApiService)) as SupplierApiService;
                        if (supplierApiService == null)
                        {
                            POS.UI.Components.DialogService.Warning("Suppliers", "Supplier service not available.");
                            return;
                        }
                        var supplierViewModel = new SupplierViewModel(supplierApiService);
                        var supplierView = new SupplierListView { DataContext = supplierViewModel };
                        MainContent.Content = supplierView;
                        HeaderTitle.Text = "Suppliers";
                        break;
                    
                    case "BtnPurchaseOrderList":
                        var purchaseOrderApiService = App.ServiceProvider?.GetService(typeof(PurchaseOrderApiService)) as PurchaseOrderApiService;
                        if (purchaseOrderApiService == null)
                        {
                            POS.UI.Components.DialogService.Warning("Purchase Orders", "Purchase Order service not available.");
                            return;
                        }
                        var purchaseOrderViewModel = new PurchaseOrderListViewModel(purchaseOrderApiService);
                        var purchaseOrderView = new PurchaseOrderListView { DataContext = purchaseOrderViewModel };
                        MainContent.Content = purchaseOrderView;
                        HeaderTitle.Text = "Purchase Orders";
                        break;
                    
                    case "BtnPurchaseEntryList":
                        var purchaseEntryApiService = App.ServiceProvider?.GetService(typeof(PurchaseEntryApiService)) as PurchaseEntryApiService;
                        if (purchaseEntryApiService == null)
                        {
                            POS.UI.Components.DialogService.Warning("Purchase Entry", "Purchase Entry service not available.");
                            return;
                        }
                        var purchaseEntryViewModel = new PurchaseEntryListViewModel(purchaseEntryApiService);
                        var purchaseEntryView = new PurchaseEntryListView { DataContext = purchaseEntryViewModel };
                        MainContent.Content = purchaseEntryView;
                        HeaderTitle.Text = "Purchase Entry (GRN)";
                        break;
                    
                    case "BtnPurchaseReturnList":
                        var purchaseReturnApiService = App.ServiceProvider?.GetService(typeof(PurchaseReturnApiService)) as PurchaseReturnApiService;
                        if (purchaseReturnApiService == null)
                        {
                            POS.UI.Components.DialogService.Warning("Purchase Return", "Purchase Return service not available.");
                            return;
                        }
                        var purchaseReturnViewModel = new PurchaseReturnListViewModel(purchaseReturnApiService);
                        var purchaseReturnView = new PurchaseReturnListView { DataContext = purchaseReturnViewModel };
                        MainContent.Content = purchaseReturnView;
                        HeaderTitle.Text = "Purchase Return";
                        break;
                    
                    case "BtnSupplierPayments":
                        var supplierPaymentApiService = App.ServiceProvider?.GetService(typeof(SupplierPaymentApiService)) as SupplierPaymentApiService;
                        var supplierApiForPayments = App.ServiceProvider?.GetService(typeof(SupplierApiService)) as SupplierApiService;
                        if (supplierPaymentApiService == null || supplierApiForPayments == null)
                        {
                            POS.UI.Components.DialogService.Warning("Supplier Payments", "Supplier Payment service not available.");
                            return;
                        }
                        var supplierPaymentViewModel = new SupplierPaymentViewModel(supplierPaymentApiService, supplierApiForPayments);
                        var supplierPaymentView = new SupplierPaymentView { DataContext = supplierPaymentViewModel };
                        MainContent.Content = supplierPaymentView;
                        HeaderTitle.Text = "Supplier Payments";
                        break;
                    
                    case "BtnSupplierLedger":
                        var supplierApiForLedger = App.ServiceProvider?.GetService(typeof(SupplierApiService)) as SupplierApiService;
                        if (supplierApiForLedger == null)
                        {
                            POS.UI.Components.DialogService.Warning("Supplier Ledger", "Supplier service not available.");
                            return;
                        }
                        var supplierLedgerViewModel = new SupplierLedgerViewModel(supplierApiForLedger);
                        var supplierLedgerView = new SupplierLedgerView { DataContext = supplierLedgerViewModel };
                        MainContent.Content = supplierLedgerView;
                        HeaderTitle.Text = "Supplier Ledger";
                        break;
                }
            }
        }

        // ================= INVENTORY SUBMENU =================
        private void SubMenuInventory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            PopupInventory.IsOpen = false;
            
            switch (btn.Name)
            {
                case "BtnStockAdjustments":
                    ShowStockAdjustmentList();
                    break;
                case "BtnItemLedger":
                    ShowItemLedger();
                    break;
                case "BtnLabelPrinting":
                    ShowLabelPrinting();
                    break;
            }
        }

        private void ShowItemLedger()
        {
            var ledgerApi = App.ServiceProvider?.GetService(typeof(ItemLedgerApiService)) as ItemLedgerApiService;
            var productApi = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;
            
            if (ledgerApi == null || productApi == null)
            {
                POS.UI.Components.DialogService.Warning("Item Ledger", "Required services not available.");
                return;
            }

            var viewModel = new Modules.Inventory.ItemLedger.ItemLedgerViewModel(ledgerApi, productApi);
            var view = new Modules.Inventory.ItemLedger.ItemLedgerView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "Item Ledger";
        }

        private void ShowLabelPrinting()
        {
            var productApi = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;
            var purchaseEntryApi = App.ServiceProvider?.GetService(typeof(PurchaseEntryApiService)) as PurchaseEntryApiService;
            var printService = App.ServiceProvider?.GetService(typeof(IPrintService)) as IPrintService;
            
            if (productApi == null || purchaseEntryApi == null || printService == null)
            {
                POS.UI.Components.DialogService.Warning("Label Printing", "Required services not available.");
                return;
            }

            var viewModel = new Modules.Inventory.LabelPrinting.LabelPrintingViewModel(productApi, purchaseEntryApi, printService);
            var view = new Modules.Inventory.LabelPrinting.LabelPrintingView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "Label Printing";
        }

        private void ShowStockAdjustmentList()
        {
            var adjustmentService = App.ServiceProvider?.GetService(typeof(StockAdjustmentApiService)) as StockAdjustmentApiService;
            var productService = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;
            var stockService = App.ServiceProvider?.GetService(typeof(StockApiService)) as StockApiService;

            if (adjustmentService == null)
            {
                POS.UI.Components.DialogService.Warning("Stock Adjustment", "Stock Adjustment service not available.");
                return;
            }

            var listViewModel = new StockAdjustmentListViewModel(adjustmentService);
            listViewModel.RequestAddNew += () => ShowCreateStockAdjustment(adjustmentService, productService, stockService);
            listViewModel.RequestView += (adjustment) => ShowStockAdjustmentDetails(adjustment);

            var listView = new StockAdjustmentListView { DataContext = listViewModel };
            MainContent.Content = listView;
            HeaderTitle.Text = "Stock Adjustments";
        }

        private void ShowCreateStockAdjustment(
            StockAdjustmentApiService adjustmentService,
            ProductApiService? productService,
            StockApiService? stockService)
        {
            if (productService == null || stockService == null)
            {
                POS.UI.Components.DialogService.Warning("Stock Adjustment", "Required services not available.");
                return;
            }

            var createViewModel = new CreateStockAdjustmentViewModel(adjustmentService, productService, stockService);
            
            // Return to list view after successful save
            createViewModel.AdjustmentSaved += (adjustment) =>
            {
                ShowStockAdjustmentList();
            };
            
            // Return to list view when cancelled
            createViewModel.RequestClose += () =>
            {
                ShowStockAdjustmentList();
            };

            var createView = new CreateStockAdjustmentView { DataContext = createViewModel };
            MainContent.Content = createView;
            HeaderTitle.Text = "Create Stock Adjustment";
        }

        private void ShowStockAdjustmentDetails(POS.Shared.Models.StockAdjustmentDto adjustment)
        {
            // For now, just show a message with details
            // Could be expanded to a full detail view later
            POS.UI.Components.DialogService.Info(
                $"Adjustment: {adjustment.ReferenceNo}",
                $"Date: {adjustment.AdjustmentDate:dd MMM yyyy}\n" +
                $"Reason: {adjustment.Reason}\n" +
                $"Status: {adjustment.Status}\n" +
                $"Items: {adjustment.Items.Count}\n" +
                $"Total Value: ₹{adjustment.TotalValue:N2}\n" +
                $"Adjusted By: {adjustment.AdjustedBy}\n" +
                $"Remarks: {adjustment.Remarks ?? "-"}");
        }

        // ================= REPORTS SUBMENU =================
        private void SubMenuReports_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            PopupReports.IsOpen = false;

            switch (btn.Name)
            {
                case "BtnSalesReportDayPeriod":
                    MainContent.Content = new SalesReportView();
                    HeaderTitle.Text = "Sales Report - Day / Month / Period";
                    break;
                case "BtnSalesReportItemWise":
                    MainContent.Content = new ItemSalesView();
                    HeaderTitle.Text = "Sales Report - Item Wise";
                    break;
                case "BtnSalesReportCategoryWise":
                    MainContent.Content = new SalesReportView();
                    HeaderTitle.Text = "Sales Report - Category Wise";
                    break;
                case "BtnSalesReportGst":
                    MainContent.Content = new GstReportView();
                    HeaderTitle.Text = "GST Reports";
                    break;
                case "BtnInventoryStockSummary":
                    MainContent.Content = new LowStockReportView();
                    HeaderTitle.Text = "Inventory Report - Stock Summary";
                    break;
                case "BtnInventoryFastSlowMoving":
                case "BtnInventoryExpiry":
                case "BtnPurchaseReports":
                case "BtnCustomerReports":
                case "BtnSupplierReports":
                case "BtnStaffPerformanceReports":
                    POS.UI.Components.DialogService.Info("Reports", "This report is not yet implemented.");
                    break;
                case "BtnProfitLossReport":
                    MainContent.Content = new ProfitLossReportView();
                    HeaderTitle.Text = "Profit && Loss";
                    break;
                case "BtnAuditLogs":
                    MainContent.Content = new AuditLogView();
                    HeaderTitle.Text = "Audit Log";
                    break;
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

        // ================= USERS SUBMENU =================
        private void SubMenuUsers_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            PopupUsers.IsOpen = false;

            switch (btn.Name)
            {
                case "BtnUserList":
                    ShowUserList();
                    break;
                case "BtnRolesPermissions":
                    ShowRolesPermissions();
                    break;
            }
        }

        private void ShowUserList()
        {
            var userApiService = App.ServiceProvider?.GetService(typeof(UserApiService)) as UserApiService;
            if (userApiService == null)
            {
                Components.DialogService.Warning("User Management", "User service not available.");
                return;
            }

            var viewModel = new UserListViewModel(userApiService);
            var view = new UserListView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "User Management";
        }

        private void ShowRolesPermissions()
        {
            var userApiService = App.ServiceProvider?.GetService(typeof(UserApiService)) as UserApiService;
            if (userApiService == null)
            {
                Components.DialogService.Warning("Roles & Permissions", "User service not available.");
                return;
            }

            var viewModel = new RoleManagerViewModel(userApiService);
            var view = new RoleManagerView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "Roles & Permissions";
        }

        // ================= PAYMENTS SUBMENU =================
        private void SubMenuPayments_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            PopupPayments.IsOpen = false;

            switch (btn.Name)
            {
                case "BtnCashManagement":
                    ShowCashManagement();
                    break;
            }
        }

        private void ShowCashManagement()
        {
            var cashService = App.ServiceProvider?.GetService(typeof(CashTransactionApiService)) as CashTransactionApiService;
            if (cashService == null)
            {
                Components.DialogService.Warning("Cash Management", "Cash service not available.");
                return;
            }

            var viewModel = new CashTransactionViewModel(cashService);
            var view = new CashTransactionView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "Cash Management";
        }

        // ================= ORGANIZATION SUBMENU =================
        private void SubMenuOrganization_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            PopupOrganization.IsOpen = false;

            switch (btn.Name)
            {
                case "BtnCompanyProfile":
                    ShowCompanyProfile();
                    break;
            }
        }

        private void ShowCompanyProfile()
        {
            var companyService = App.ServiceProvider?.GetService(typeof(CompanyProfileApiService)) as CompanyProfileApiService;
            if (companyService == null)
            {
                Components.DialogService.Warning("Company Profile", "Company service not available.");
                return;
            }

            var viewModel = new CompanyProfileViewModel(companyService);
            var view = new CompanyProfileView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "Company Profile";
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

        private void NavigateToDashboardView()
        {
            MainContent.Content = ViewResolver.Resolve("DashboardView");
            HeaderTitle.Text = "Dashboard";
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control
                && e.Key == System.Windows.Input.Key.Home)
            {
                NavigateToDashboardView();
                e.Handled = true;
            }
        }

        // ================= SALES RETURN MODULE =================
        private void ShowSalesReturnList()
        {
            var saleReturnService = App.ServiceProvider?.GetService(typeof(SaleReturnApiService)) as SaleReturnApiService;
            if (saleReturnService == null)
            {
                Components.DialogService.Warning("Sales Returns", "Sale Return service not available.");
                return;
            }

            var viewModel = new SaleReturnListViewModel(saleReturnService);
            viewModel.RequestAddNew += () => ShowCreateSaleReturn(saleReturnService);
            viewModel.RequestView += (sr) =>
            {
                Components.DialogService.Info($"Return: {sr.ReturnNumber}",
                    $"Date: {sr.ReturnDate:dd MMM yyyy}\nInvoice: {sr.OriginalBillNumber}\nCustomer: {sr.CustomerName}\n" +
                    $"Amount: {sr.RefundAmount:N2}\nRefund Mode: {sr.RefundMode}\nStatus: {sr.Status}\nItems: {sr.Items.Count}");
            };

            var view = new SaleReturnListView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "Sales Returns";
        }

        private void ShowCreateSaleReturn(SaleReturnApiService service)
        {
            var viewModel = new CreateSaleReturnViewModel(service);
            viewModel.ReturnSaved += () => ShowSalesReturnList();
            viewModel.RequestClose += () => ShowSalesReturnList();

            var view = new CreateSaleReturnView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "Create Sales Return";
        }

        // ================= CUSTOMER OUTSTANDING MODULE =================
        private void ShowCustomerOutstanding()
        {
            var customerPaymentService = App.ServiceProvider?.GetService(typeof(CustomerPaymentApiService)) as CustomerPaymentApiService;
            if (customerPaymentService == null)
            {
                Components.DialogService.Warning("Customer Outstanding", "Customer Payment service not available.");
                return;
            }

            var viewModel = new CustomerOutstandingViewModel(customerPaymentService);
            var view = new CustomerOutstandingView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "Customer Outstanding / Dues";
        }

        // ================= QUOTATION MODULE =================
        private void ShowQuotationList()
        {
            var quotationService = App.ServiceProvider?.GetService(typeof(QuotationApiService)) as QuotationApiService;
            if (quotationService == null)
            {
                Components.DialogService.Warning("Quotations", "Quotation service not available.");
                return;
            }

            var productService = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;
            var customerService = App.ServiceProvider?.GetService(typeof(CustomerApiService)) as CustomerApiService;

            var viewModel = new QuotationListViewModel(quotationService);
            viewModel.RequestAddNew += () => ShowQuotationEntry(quotationService, productService, customerService, null);
            viewModel.RequestEdit += (q) => ShowQuotationEntry(quotationService, productService, customerService, q);
            viewModel.RequestView += (q) =>
            {
                Components.DialogService.Info($"Quotation: {q.QuotationNumber}",
                    $"Date: {q.QuotationDate:dd MMM yyyy}\nValid Until: {q.ValidUntil:dd MMM yyyy}\n" +
                    $"Customer: {q.CustomerName}\nAmount: {q.TotalAmount:N2}\nStatus: {q.Status}\nItems: {q.Items.Count}");
            };

            var view = new QuotationListView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = "Quotations";
        }

        private async void ShowQuotationEntry(QuotationApiService quotationService, ProductApiService? productService, CustomerApiService? customerService, QuotationDto? editQuotation)
        {
            if (productService == null || customerService == null)
            {
                Components.DialogService.Warning("Quotation", "Required services not available.");
                return;
            }

            var viewModel = new QuotationEntryViewModel(quotationService, productService, customerService);
            viewModel.QuotationSaved += () => ShowQuotationList();
            viewModel.RequestClose += () => ShowQuotationList();

            if (editQuotation != null)
                await viewModel.LoadForEdit(editQuotation.Id);

            var view = new QuotationEntryView { DataContext = viewModel };
            MainContent.Content = view;
            HeaderTitle.Text = editQuotation != null ? $"Edit Quotation: {editQuotation.QuotationNumber}" : "New Quotation";
        }

        // ================= MENU BAR HANDLERS (File, Sales, Reports, Settings, Help) =================


    }
}
