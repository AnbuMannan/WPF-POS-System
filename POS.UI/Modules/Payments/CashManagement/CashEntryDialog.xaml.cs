using System.Windows;
using System.Windows.Media;

namespace POS.UI.Modules.Payments.CashManagement
{
    public partial class CashEntryDialog : Window
    {
        public string DialogTitle { get; }
        public decimal Amount { get; private set; }
        public string? Description { get; private set; }
        public string? Category { get; private set; }
        public string? Remarks { get; private set; }

        private readonly bool _isCashIn;

        public CashEntryDialog(string title, bool isCashIn)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            _isCashIn = isCashIn;

            // Set button color based on type
            SaveButton.Background = isCashIn 
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96))   // Green for Cash In
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60)); // Red for Cash Out

            // Set categories based on type
            if (isCashIn)
            {
                CategoryBox.ItemsSource = new[] { "Opening Balance", "Cash Sales", "Petty Cash", "Other" };
            }
            else
            {
                CategoryBox.ItemsSource = new[] { "Petty Cash", "Office Supplies", "Transport", "Food", "Utilities", "Other" };
            }
            CategoryBox.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountBox.Text, out var amount) || amount <= 0)
            {
                Components.DialogService.Warning("Validation", "Please enter a valid amount greater than zero");
                return;
            }

            Amount = amount;
            Description = DescriptionBox.Text.Trim();
            Category = CategoryBox.SelectedItem?.ToString();
            Remarks = RemarksBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
