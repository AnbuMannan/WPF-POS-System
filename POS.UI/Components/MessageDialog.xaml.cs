using System.Windows;

namespace POS.UI.Components
{
    public enum DialogIcon
    {
        Info,
        Warning,
        Error,
        Success
    }

    public enum DialogButtons
    {
        Ok,
        OkCancel,
        YesNo
    }

    public partial class MessageDialog : Window
    {
        public string IconGlyph { get; set; }
        public string TitleText { get; set; }
        public string MessageText { get; set; }

        public string PrimaryButtonText { get; set; }
        public string SecondaryButtonText { get; set; }
        public Visibility SecondaryVisibility { get; set; }

        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;
        private readonly DialogButtons _buttons;

        public MessageDialog(string title, string message, DialogIcon icon, DialogButtons buttons)
        {
            InitializeComponent();

            TitleText = title;
            MessageText = message;
            _buttons = buttons;

            IconGlyph = GetIconGlyph(icon);
            ConfigureButtons(buttons);

            DataContext = this;
        }

        private static string GetIconGlyph(DialogIcon icon)
        {
            switch (icon)
            {
                case DialogIcon.Info:
                    return "\uE946";
                case DialogIcon.Warning:
                    return "\uE7BA";
                case DialogIcon.Error:
                    return "\uEA39";
                case DialogIcon.Success:
                    return "\uE73E";
                default:
                    return "\uE946";
            }
        }

        private void ConfigureButtons(DialogButtons buttons)
        {
            switch (buttons)
            {
                case DialogButtons.Ok:
                    PrimaryButtonText = "OK";
                    SecondaryVisibility = Visibility.Collapsed;
                    break;
                case DialogButtons.OkCancel:
                    PrimaryButtonText = "OK";
                    SecondaryButtonText = "Cancel";
                    SecondaryVisibility = Visibility.Visible;
                    break;
                case DialogButtons.YesNo:
                    PrimaryButtonText = "Yes";
                    SecondaryButtonText = "No";
                    SecondaryVisibility = Visibility.Visible;
                    break;
            }
        }

        private void OnPrimaryClicked(object sender, RoutedEventArgs e)
        {
            switch (_buttons)
            {
                case DialogButtons.Ok:
                    Result = MessageBoxResult.OK;
                    break;
                case DialogButtons.OkCancel:
                    Result = MessageBoxResult.OK;
                    break;
                case DialogButtons.YesNo:
                    Result = MessageBoxResult.Yes;
                    break;
            }
            DialogResult = true;
            Close();
        }

        private void OnSecondaryClicked(object sender, RoutedEventArgs e)
        {
            switch (_buttons)
            {
                case DialogButtons.OkCancel:
                    Result = MessageBoxResult.Cancel;
                    break;
                case DialogButtons.YesNo:
                    Result = MessageBoxResult.No;
                    break;
                default:
                    Result = MessageBoxResult.None;
                    break;
            }
            DialogResult = false;
            Close();
        }
    }
}
