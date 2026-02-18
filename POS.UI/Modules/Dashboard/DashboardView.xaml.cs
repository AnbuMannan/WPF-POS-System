using POS.UI.Core.Services;
using System;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();

        var service = App.ServiceProvider?.GetService(typeof(DashboardApiService)) as DashboardApiService;
        if (service == null)
            throw new InvalidOperationException("DashboardApiService is not registered.");

        var vm = new DashboardViewModel(service);
        DataContext = vm;
        _ = vm.InitializeAsync();
    }
}

