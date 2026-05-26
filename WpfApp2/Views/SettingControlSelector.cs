using System.Windows;
using System.Windows.Controls;
using Som3a_WPF_UI.Services;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Views
{
    public sealed class SettingControlSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is not SettingControlViewModel vm)
                return base.SelectTemplate(item, container);

            var key = vm.ValueType switch
            {
                SettingValueType.String => "SettingTemplate_String",
                SettingValueType.Integer => "SettingTemplate_Integer",
                SettingValueType.Decimal => "SettingTemplate_Decimal",
                SettingValueType.Boolean => "SettingTemplate_Boolean",
                SettingValueType.Enum => "SettingTemplate_Enum",
                SettingValueType.Color => "SettingTemplate_Color",
                SettingValueType.FilePath => "SettingTemplate_FilePath",
                SettingValueType.Secret => "SettingTemplate_Secret",
                _ => "SettingTemplate_String"
            };

            if (container is FrameworkElement element)
                return element.TryFindResource(key) as DataTemplate ?? base.SelectTemplate(item, container);

            return base.SelectTemplate(item, container);
        }
    }

    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper));

        public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);
        public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);

        private static bool GetIsUpdating(DependencyObject d) => (bool)(d.GetValue(IsUpdatingProperty) ?? false);
        private static void SetIsUpdating(DependencyObject d, bool value) => d.SetValue(IsUpdatingProperty, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox && !GetIsUpdating(passwordBox))
            {
                passwordBox.PasswordChanged -= OnPasswordChanged;
                passwordBox.Password = (string)e.NewValue ?? string.Empty;
                passwordBox.PasswordChanged += OnPasswordChanged;
            }
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetIsUpdating(passwordBox, true);
                SetBoundPassword(passwordBox, passwordBox.Password);
                SetIsUpdating(passwordBox, false);
            }
        }
    }
}
