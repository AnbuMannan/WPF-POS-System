using POS.UI.Core.Services;
using System.Windows.Controls;

namespace POS.UI.Modules.Finance.Expenses;

public partial class ExpenseView : System.Windows.Controls.UserControl
{
    public ExpenseView()
    {
        InitializeComponent();
        var svc = App.ServiceProvider?.GetService(typeof(ExpenseApiService)) as ExpenseApiService;
        if (svc != null)
        {
            var vm = new ExpenseViewModel(svc);
            DataContext = vm;
            _ = vm.InitializeAsync();
        }
    }
}
