using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Modules.Billing.BillingScreen;
using MessageBox = System.Windows.MessageBox;

namespace POS.UI.Modules.Billing.DraftBills
{
    public class DraftBillsListViewModel : ViewModelBase
    {
        private readonly BillingApiService _billingApi;
        private readonly BillingViewModel _billingViewModel;
        private ObservableCollection<DraftBillDto> _draftBills = new();
        private DraftBillDto? _selectedDraftBill;

        public ObservableCollection<DraftBillDto> DraftBills
        {
            get => _draftBills;
            set { _draftBills = value; OnPropertyChanged(); }
        }

        public DraftBillDto? SelectedDraftBill
        {
            get => _selectedDraftBill;
            set { _selectedDraftBill = value; OnPropertyChanged(); }
        }

        public ICommand RetrieveCommand { get; }
        public ICommand DeleteCommand { get; }

        public Func<bool>? ConfirmRetrieveWhenCartHasItems { get; set; }
        public Action<DraftBillDto>? OnRetrieveDraft { get; set; }

        public DraftBillsListViewModel(BillingApiService billingApi, BillingViewModel billingViewModel)
        {
            _billingApi = billingApi;
            _billingViewModel = billingViewModel;

            RetrieveCommand = new RelayCommand(async () => await RetrieveDraft(), () => SelectedDraftBill != null);
            DeleteCommand = new RelayCommand(async () => await DeleteDraft(), () => SelectedDraftBill != null);
        }

        public async Task LoadDraftBillsAsync()
        {
            try
            {
                var bills = await _billingApi.GetDraftBillsAsync();
                DraftBills = new ObservableCollection<DraftBillDto>(bills ?? new List<DraftBillDto>());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load draft bills: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RetrieveDraft()
        {
            if (SelectedDraftBill == null) return;

            if (ConfirmRetrieveWhenCartHasItems != null && !ConfirmRetrieveWhenCartHasItems())
                return;

            try
            {
                OnRetrieveDraft?.Invoke(SelectedDraftBill);
                await _billingApi.DeleteDraftBillAsync(SelectedDraftBill.Id);
                DraftBills.Remove(SelectedDraftBill);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to retrieve draft: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteDraft()
        {
            if (SelectedDraftBill == null) return;

            if (MessageBox.Show("Delete this draft?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                await _billingApi.DeleteDraftBillAsync(SelectedDraftBill.Id);
                DraftBills.Remove(SelectedDraftBill);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete draft: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
