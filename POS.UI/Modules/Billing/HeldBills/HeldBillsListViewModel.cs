using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Modules.Billing.BillingScreen;
using MessageBox = System.Windows.MessageBox;

namespace POS.UI.Modules.Billing.HeldBills
{
    public class HeldBillsListViewModel : ViewModelBase
    {
        private readonly BillingApiService _billingApi;
        private readonly BillingViewModel _billingViewModel;
        private ObservableCollection<HeldBillDto> _heldBills = new();
        private HeldBillDto? _selectedHeldBill;

        public ObservableCollection<HeldBillDto> HeldBills
        {
            get => _heldBills;
            set { _heldBills = value; OnPropertyChanged(); }
        }

        public HeldBillDto? SelectedHeldBill
        {
            get => _selectedHeldBill;
            set { _selectedHeldBill = value; OnPropertyChanged(); }
        }

        public ICommand RetrieveCommand { get; }
        public ICommand DeleteCommand { get; }

        public Func<bool>? ConfirmRetrieveWhenCartHasItems { get; set; }
        public Action<HeldBillDto>? OnRetrieveHeldBill { get; set; }

        public HeldBillsListViewModel(BillingApiService billingApi, BillingViewModel billingViewModel)
        {
            _billingApi = billingApi;
            _billingViewModel = billingViewModel;

            RetrieveCommand = new RelayCommand(async () => await RetrieveHeldBill(), () => SelectedHeldBill != null);
            DeleteCommand = new RelayCommand(async () => await DeleteHeldBill(), () => SelectedHeldBill != null);
        }

        public async Task LoadHeldBillsAsync()
        {
            try
            {
                var bills = await _billingApi.GetHeldBillsAsync();
                HeldBills = new ObservableCollection<HeldBillDto>(bills ?? new List<HeldBillDto>());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load held bills: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RetrieveHeldBill()
        {
            if (SelectedHeldBill == null) return;

            if (ConfirmRetrieveWhenCartHasItems != null && !ConfirmRetrieveWhenCartHasItems())
                return;

            try
            {
                OnRetrieveHeldBill?.Invoke(SelectedHeldBill);
                await _billingApi.DeleteHeldBillAsync(SelectedHeldBill.Id);
                HeldBills.Remove(SelectedHeldBill);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to retrieve held bill: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteHeldBill()
        {
            if (SelectedHeldBill == null) return;

            if (MessageBox.Show("Delete this held bill?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                await _billingApi.DeleteHeldBillAsync(SelectedHeldBill.Id);
                HeldBills.Remove(SelectedHeldBill);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete held bill: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
