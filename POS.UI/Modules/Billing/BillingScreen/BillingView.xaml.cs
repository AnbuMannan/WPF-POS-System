using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using POS.UI.Core.Services;
using UserControl = System.Windows.Controls.UserControl;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using POS.UI.Modules.Billing.CustomerDisplay;
using POS.UI.Modules.Billing.HoldBill;
using POS.UI.Modules.Billing.DraftBill;
using POS.UI.Modules.Billing.HeldBills;
using POS.UI.Modules.Billing.DraftBills;
using POS.UI.Modules.Admin.Customers;

namespace POS.UI.Modules.Billing.BillingScreen
{
    public partial class BillingView : UserControl
    {
        private static CustomerDisplayWindow? _customerDisplayWindow;
        public BillingView()
        {
            InitializeComponent();
            Loaded += BillingView_Loaded;

            try
            {
                if (App.ServiceProvider != null)
                {
                    var billingApi = App.ServiceProvider.GetService(typeof(BillingApiService)) as BillingApiService;
                    var productApi = App.ServiceProvider.GetService(typeof(ProductApiService)) as ProductApiService;
                    var customerApi = App.ServiceProvider.GetService(typeof(CustomerApiService)) as CustomerApiService;
                    var taxProfileApi = App.ServiceProvider.GetService(typeof(TaxProfileApiService)) as TaxProfileApiService;
                    var uomApi = App.ServiceProvider.GetService(typeof(UomApiService)) as UomApiService;
                    var printService = App.ServiceProvider.GetService(typeof(IPrintService)) as IPrintService;
                    var printSettings = App.ServiceProvider.GetService(typeof(IPrintSettingsService)) as IPrintSettingsService;
                    var emailReceipt = App.ServiceProvider.GetService(typeof(IEmailReceiptService)) as IEmailReceiptService;
                    var auditLogApi = App.ServiceProvider.GetService(typeof(AuditLogApiService)) as AuditLogApiService;
                    if (billingApi != null && productApi != null && customerApi != null && taxProfileApi != null && uomApi != null)
                    {
                        var vm = new BillingViewModel(billingApi, productApi, customerApi, taxProfileApi, uomApi, printService, printSettings, auditLogApi);
                        var owner = Window.GetWindow(this) as Window;

                        vm.OpenPaymentDialogAsync = async (total) =>
                        {
                            var dialog = new POS.UI.Modules.Billing.PaymentDialog.PaymentDialog(billingApi, total) { Owner = owner };
                            return dialog.ShowDialog() == true ? dialog.ViewModel.CompletedPayments : null;
                        };

                        vm.OpenHoldBillDialog = () =>
                        {
                            var d = new HoldBillDialog(billingApi, vm) { Owner = owner };
                            d.ShowDialog();
                        };

                        vm.OpenDraftBillDialog = () =>
                        {
                            var d = new DraftBillDialog(billingApi, vm) { Owner = owner };
                            d.ShowDialog();
                        };

                        vm.ShowHeldBills = () =>
                        {
                            var w = new HeldBillsListView(billingApi, vm) { Owner = owner };
                            w.ViewModel.ConfirmRetrieveWhenCartHasItems = () =>
                                vm.CartItems.Count == 0 || MessageBox.Show("Current cart has items. Replace with held bill?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                            w.ViewModel.OnRetrieveHeldBill = (dto) => { vm.LoadCartFromJson(dto.CartData); w.Close(); };
                            w.ShowDialog();
                        };

                        vm.ShowDraftBills = () =>
                        {
                            var w = new DraftBillsListView(billingApi, vm) { Owner = owner };
                            w.ViewModel.ConfirmRetrieveWhenCartHasItems = () =>
                                vm.CartItems.Count == 0 || MessageBox.Show("Current cart has items. Replace with draft?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                            w.ViewModel.OnRetrieveDraft = (dto) => { vm.LoadCartFromJson(dto.CartData); w.Close(); };
                            w.ShowDialog();
                        };

                        vm.ShowReceiptPreview = (receipt) =>
                        {
                            if (printService == null) return;
                            var dialog = new POS.UI.Modules.Billing.ReceiptPreview.PrintPreviewDialog(receipt, printService, emailReceipt) { Owner = owner };
                            dialog.ShowDialog();
                        };

                        vm.FocusSearchRequested = () => ProductSearchControl?.FocusSearchBox();
                        vm.FocusCustomerRequested = () => CustomerCombo?.Focus();
                        vm.FocusDiscountRequested = () => DiscountBox?.Focus();
                        vm.ToggleQuickProductsRequested = () =>
                        {
                            if (QuickProductsPanel != null)
                                QuickProductsPanel.Visibility = QuickProductsPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                        };
                        vm.ShowShortcutsRequested = () =>
                        {
                            var shortcutWin = new POS.UI.Modules.Billing.Shortcuts.ShortcutKeysWindow { Owner = owner };
                            shortcutWin.ShowDialog();
                        };
                        vm.ReprintReceiptRequested = () => ReprintLastReceipt(owner, printSettings, billingApi, printService, emailReceipt);
                        vm.RequestNewBill = () =>
                        {
                            if (MessageBox.Show("New bill? This will clear the cart.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                vm.ClearCartCommand.Execute(null);
                        };
                        vm.ToggleFullScreenRequested = () =>
                        {
                            if (owner != null)
                                owner.WindowState = owner.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                        };

                        DataContext = vm;

                        // Only auto-show Customer Display when a secondary monitor exists.
                        // On single-monitor setups it would open fullscreen with no close button and block the app.
                        if (HasSecondaryMonitor())
                        {
                            _customerDisplayWindow ??= new CustomerDisplayWindow();
                            _customerDisplayWindow.Closed += (s, _) => _customerDisplayWindow = null;
                            _customerDisplayWindow.SetSource(vm);
                            _customerDisplayWindow.Show();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Components.DialogService.Error("Initialization Error", $"Failed to initialize Billing screen: {ex.Message}");
            }
        }

        private void BillingView_Loaded(object sender, RoutedEventArgs e)
        {
            ProductSearchControl?.FocusSearchBox();
            if (DataContext is BillingViewModel vm)
            {
                vm.LoadCustomersCommand?.Execute(null);
                vm.CartItems.CollectionChanged += CartItems_CollectionChanged;
            }
        }

        private void BillingView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not BillingViewModel vm) return;

            // Don't intercept if user is typing in a TextBox (except for function keys and specific shortcuts)
            bool isTypingInTextBox = e.OriginalSource is System.Windows.Controls.TextBox && 
                                      e.Key != Key.F1 && e.Key != Key.F2 && e.Key != Key.F3 && 
                                      e.Key != Key.F4 && e.Key != Key.F5 && e.Key != Key.F9 && 
                                      e.Key != Key.F10 && e.Key != Key.F11 && e.Key != Key.F12 &&
                                      (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) == ModifierKeys.None;

            if (isTypingInTextBox) return;

            // Function keys
            if (e.Key == Key.F1)
            {
                vm.FocusSearchCommand?.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F2)
            {
                vm.OpenHoldBillDialogCommand?.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F3)
            {
                vm.OpenDraftBillDialogCommand?.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F4)
            {
                vm.ClearCartCommand?.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F5)
            {
                vm.LoadCustomersCommand?.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F9)
            {
                vm.ShowHeldBillsCommand?.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F10)
            {
                vm.ShowDraftBillsCommand?.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F11)
            {
                vm.ToggleFullScreenCommand?.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F12)
            {
                vm.ProceedToPaymentCommand?.Execute(null);
                e.Handled = true;
                return;
            }

            // Ctrl + Key shortcuts
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.H:
                        vm.OpenHoldBillDialogCommand?.Execute(null);
                        e.Handled = true;
                        break;
                    case Key.S:
                        vm.OpenDraftBillDialogCommand?.Execute(null);
                        e.Handled = true;
                        break;
                    case Key.D:
                        vm.FocusDiscountCommand?.Execute(null);
                        e.Handled = true;
                        break;
                    case Key.X:
                        vm.ClearCartCommand?.Execute(null);
                        e.Handled = true;
                        break;
                    case Key.P:
                        vm.ReprintLastReceiptCommand?.Execute(null);
                        e.Handled = true;
                        break;
                    case Key.N:
                        vm.NewBillCommand?.Execute(null);
                        e.Handled = true;
                        break;
                    case Key.R:
                        vm.ShowHeldBillsCommand?.Execute(null);
                        e.Handled = true;
                        break;
                    case Key.F:
                        vm.FocusSearchCommand?.Execute(null);
                        e.Handled = true;
                        break;
                    case Key.OemQuestion: // Ctrl + ?
                        vm.ShowShortcutsCommand?.Execute(null);
                        e.Handled = true;
                        break;
                }
            }

            // Alt + Key shortcuts
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                switch (e.Key)
                {
                    case Key.C:
                        vm.FocusCustomerCommand?.Execute(null);
                        e.Handled = true;
                        break;
                    case Key.P:
                        vm.ProceedToPaymentCommand?.Execute(null);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void CartItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null || e.NewItems.Count == 0 || e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                return;
            var newItem = e.NewItems[0];
            Dispatcher.BeginInvoke(new Action(() => FlashRowForNewItem(newItem)), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void FlashRowForNewItem(object item)
        {
            if (CartTable?.ItemContainerGenerator.ContainerFromItem(item) is not System.Windows.Controls.DataGridRow row)
                return;
            var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 230, 201));
            row.Background = brush;
            var storyboard = (System.Windows.Media.Animation.Storyboard)FindResource("RowAddedFlash");
            storyboard = storyboard.Clone();
            System.Windows.Media.Animation.Storyboard.SetTarget(storyboard, row);
            storyboard.Completed += (_, _) => row.ClearValue(System.Windows.Controls.DataGridRow.BackgroundProperty);
            storyboard.Begin();
        }

        private void BtnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this) as Window;
            var form = new CustomerFormView(null) { Owner = owner };
            if (form.ShowDialog() == true && DataContext is BillingViewModel vm)
                vm.LoadCustomersCommand?.Execute(null);
        }

        private void QuickProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag && DataContext is BillingViewModel vm)
            {
                if (vm.SelectedSearchProduct != null)
                    vm.AddProductToCart(vm.SelectedSearchProduct, 1);
            }
        }

        private void CartTable_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not System.Windows.Controls.DataGrid grid || DataContext is not BillingViewModel vm)
                return;
            if (e.Key == Key.Delete && vm.SelectedCartItem != null)
            {
                if (MessageBox.Show("Remove this item from cart?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    vm.RemoveFromCartCommand.Execute(vm.SelectedCartItem);
                e.Handled = true;
            }
        }

        /// <summary>Allow only digits and one decimal point in quantity TextBox.</summary>
        private void QuantityBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumericQuantityInput(e.Text, sender as System.Windows.Controls.TextBox);
        }

        private static bool IsNumericQuantityInput(string newText, System.Windows.Controls.TextBox? box)
        {
            if (string.IsNullOrEmpty(newText)) return true;
            foreach (var c in newText)
            {
                if (char.IsDigit(c)) continue;
                if (c == '.' && box != null && !(box.Text ?? "").Contains('.')) continue;
                return false;
            }
            return true;
        }

        private static async void ReprintLastReceipt(Window? owner,
            POS.UI.Core.Services.IPrintSettingsService? printSettings,
            BillingApiService billingApi,
            POS.UI.Core.Services.IPrintService? printService,
            POS.UI.Core.Services.IEmailReceiptService? emailReceipt)
        {
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
            var dialog = new POS.UI.Modules.Billing.ReceiptPreview.PrintPreviewDialog(receipt, printService, emailReceipt) { Owner = owner };
            dialog.ShowDialog();
        }

        private static bool HasSecondaryMonitor()
        {
            try
            {
                return System.Windows.Forms.Screen.AllScreens.Length > 1;
            }
            catch
            {
                return false;
            }
        }
    }
}
