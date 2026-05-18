using System;
using System.Windows;
using System.Windows.Input;

namespace Som3a_WPF_UI.Helpers
{
    public static class WindowBehaviorHelper
    {
        public static readonly DependencyProperty EnableDragMoveProperty =
            DependencyProperty.RegisterAttached(
                "EnableDragMove",
                typeof(bool),
                typeof(WindowBehaviorHelper),
                new PropertyMetadata(false, OnEnableDragMoveChanged));

        public static bool GetEnableDragMove(DependencyObject obj) =>
            (bool)obj.GetValue(EnableDragMoveProperty);

        public static void SetEnableDragMove(DependencyObject obj, bool value) =>
            obj.SetValue(EnableDragMoveProperty, value);

        private static void OnEnableDragMoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                }
                else
                {
                    element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                }
            }
        }

        private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                var window = Window.GetWindow((DependencyObject)sender);
                if (window != null && e.ButtonState == MouseButtonState.Pressed)
                {
                    window.DragMove();
                }
            }
        }

        public static readonly DependencyProperty CloseOnEscapeProperty =
            DependencyProperty.RegisterAttached(
                "CloseOnEscape",
                typeof(bool),
                typeof(WindowBehaviorHelper),
                new PropertyMetadata(false, OnCloseOnEscapeChanged));

        public static bool GetCloseOnEscape(DependencyObject obj) =>
            (bool)obj.GetValue(CloseOnEscapeProperty);

        public static void SetCloseOnEscape(DependencyObject obj, bool value) =>
            obj.SetValue(CloseOnEscapeProperty, value);

        private static void OnCloseOnEscapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool)e.NewValue)
                {
                    window.PreviewKeyDown += Window_PreviewKeyDown;
                }
                else
                {
                    window.PreviewKeyDown -= Window_PreviewKeyDown;
                }
            }
        }

        private static void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var window = sender as Window;
                window?.Close();
            }
        }

        public static readonly DependencyProperty IsVstoModeProperty =
            DependencyProperty.RegisterAttached(
                "IsVstoMode",
                typeof(bool),
                typeof(WindowBehaviorHelper),
                new PropertyMetadata(false));

        public static bool GetIsVstoMode(DependencyObject obj) =>
            (bool)obj.GetValue(IsVstoModeProperty);

        public static void SetIsVstoMode(DependencyObject obj, bool value) =>
            obj.SetValue(IsVstoModeProperty, value);
    }
}