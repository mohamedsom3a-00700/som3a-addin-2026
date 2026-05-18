using System.Windows;
using System.Windows.Input;

namespace Som3a_WPF_UI.Behaviors
{
    public static class EscapeCloseBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(EscapeCloseBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static bool GetEnable(DependencyObject obj) =>
            (bool)obj.GetValue(EnableProperty);

        public static void SetEnable(DependencyObject obj, bool value) =>
            obj.SetValue(EnableProperty, value);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool)e.NewValue)
                {
                    window.PreviewKeyDown += OnPreviewKeyDown;
                }
                else
                {
                    window.PreviewKeyDown -= OnPreviewKeyDown;
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                (sender as Window)?.Close();
            }
        }
    }
}