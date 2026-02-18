using System.Windows;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace POS.UI.Modules.Reports.Sales
{
    public partial class ReportFilterControl : UserControl
    {
        public static readonly DependencyProperty FromDateProperty =
            DependencyProperty.Register(nameof(FromDate), typeof(DateTime?), typeof(ReportFilterControl));

        public static readonly DependencyProperty ToDateProperty =
            DependencyProperty.Register(nameof(ToDate), typeof(DateTime?), typeof(ReportFilterControl));

        public static readonly DependencyProperty ExtraFiltersProperty =
            DependencyProperty.Register(nameof(ExtraFilters), typeof(object), typeof(ReportFilterControl));

        public static readonly DependencyProperty GenerateCommandProperty =
            DependencyProperty.Register(nameof(GenerateCommand), typeof(ICommand), typeof(ReportFilterControl));

        public DateTime? FromDate
        {
            get => (DateTime?)GetValue(FromDateProperty);
            set => SetValue(FromDateProperty, value);
        }

        public DateTime? ToDate
        {
            get => (DateTime?)GetValue(ToDateProperty);
            set => SetValue(ToDateProperty, value);
        }

        public object ExtraFilters
        {
            get => GetValue(ExtraFiltersProperty);
            set => SetValue(ExtraFiltersProperty, value);
        }

        public ICommand GenerateCommand
        {
            get => (ICommand)GetValue(GenerateCommandProperty);
            set => SetValue(GenerateCommandProperty, value);
        }

        public ReportFilterControl()
        {
            InitializeComponent();
        }
    }
}
