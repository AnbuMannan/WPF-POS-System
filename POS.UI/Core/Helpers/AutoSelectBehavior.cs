using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS.UI.Core.Helpers
{
    /// <summary>
    /// Automatically selects all text when a TextBox gets focus
    /// </summary>
    public static class AutoSelectBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(AutoSelectBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.GotKeyboardFocus += SelectAll;
                    textBox.PreviewMouseLeftButtonDown += IgnoreMouseButton;
                }
                else
                {
                    textBox.GotKeyboardFocus -= SelectAll;
                    textBox.PreviewMouseLeftButtonDown -= IgnoreMouseButton;
                }
            }
        }

        private static void SelectAll(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
                tb.SelectAll();
        }

        private static void IgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb && !tb.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                tb.Focus();
            }
        }
    }
}
