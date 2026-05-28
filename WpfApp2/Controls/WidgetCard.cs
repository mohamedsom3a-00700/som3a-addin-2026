using System;
using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Controls
{
    public class WidgetCard : ContentControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(WidgetCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(WidgetCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(WidgetCard),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(WidgetCard),
                new PropertyMetadata(null));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        static WidgetCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WidgetCard),
                new FrameworkPropertyMetadata(typeof(WidgetCard)));
        }
    }
}
