using System.Windows;

namespace POS.UI.Components
{
    public static class DialogService
    {
        public static MessageBoxResult Confirm(string title, string message)
        {
            var dlg = new MessageDialog(title, message, DialogIcon.Warning, DialogButtons.YesNo);
            dlg.Owner = Application.Current?.MainWindow;
            var res = dlg.ShowDialog();
            return dlg.Result;
        }

        public static MessageBoxResult Info(string title, string message)
        {
            var dlg = new MessageDialog(title, message, DialogIcon.Info, DialogButtons.Ok);
            dlg.Owner = Application.Current?.MainWindow;
            var res = dlg.ShowDialog();
            return dlg.Result;
        }

        public static MessageBoxResult Error(string title, string message)
        {
            var dlg = new MessageDialog(title, message, DialogIcon.Error, DialogButtons.Ok);
            dlg.Owner = Application.Current?.MainWindow;
            var res = dlg.ShowDialog();
            return dlg.Result;
        }
    }
}
