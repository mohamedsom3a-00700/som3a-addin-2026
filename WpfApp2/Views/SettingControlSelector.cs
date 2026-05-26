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
                return element.FindResource(key) as DataTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
