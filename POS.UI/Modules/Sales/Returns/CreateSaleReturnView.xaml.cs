namespace POS.UI.Modules.Sales.Returns;

public partial class CreateSaleReturnView : System.Windows.Controls.UserControl
{
    public CreateSaleReturnView()
    {
        InitializeComponent();
        Loaded += (_, _) => TxtInvoiceSearch.Focus();
    }
}
