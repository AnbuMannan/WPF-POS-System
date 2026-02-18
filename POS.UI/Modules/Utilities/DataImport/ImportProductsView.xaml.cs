using POS.UI.Core.Services;
using System;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Utilities.DataImport;

public partial class ImportProductsView : UserControl
{
    public ImportProductsView()
    {
        InitializeComponent();

        var service = App.ServiceProvider.GetService(typeof(ImportApiService)) as ImportApiService;
        if (service == null)
            throw new InvalidOperationException("ImportApiService is not registered.");

        DataContext = new ImportProductsViewModel(service);
    }
}
