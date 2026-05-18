using System.Windows;
using System.Windows.Input;

namespace Som3a_WPF_UI.Behaviors
{
    public static class DragMoveBehavior
    {
        public static readonly DependencyProperty EnableDragProperty =
            DependencyProperty.RegisterAttached(
                "EnableDrag",
                typeof(bool),
                typeof(DragMoveBehavior),
                new PropertyMetadata(false, OnEnableDragChanged));

        public static bool GetEnableDrag(DependencyObject obj) =>
            (bool)obj.GetValue(EnableDragProperty);

        public static void SetEnableDrag(DependencyObject obj, bool value) =>
            obj.SetValue(EnableDragProperty, value);

        private static void OnEnableDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.MouseLeftButtonDown += OnMouseLeftButtonDown;
                }
                else
                {
                    element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                }
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && e.ButtonState == MouseButtonState.Pressed)
            {
                var window = Window.GetWindow((DependencyObject)sender);
                window?.DragMove();
            }
        }
    }
}