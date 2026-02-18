namespace POS.UI.Modules.Sales.Quotations;

public partial class QuotationEntryView : System.Windows.Controls.UserControl
{
    public QuotationEntryView()
    {
        InitializeComponent();
        Loaded += (_, _) => TxtProductSearch?.Focus();
    }
}
