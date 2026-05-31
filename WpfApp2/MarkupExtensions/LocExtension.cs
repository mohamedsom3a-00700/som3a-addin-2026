using System;
using System.Windows;
using System.Windows.Markup;
using Som3a.Localization.Contracts;

namespace Som3a_WPF_UI.MarkupExtensions
{
    public class LocExtension : MarkupExtension
    {
        private static ILocalizationService? _service;

        public string Key { get; set; } = string.Empty;

        public LocExtension() { }

        public LocExtension(string key)
        {
            Key = key;
        }

        internal static void SetService(ILocalizationService service)
        {
            _service = service;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_service == null || string.IsNullOrEmpty(Key))
                return Key;

            var value = _service.GetString(Key);
            return string.IsNullOrEmpty(value) ? Key : value;
        }
    }
}
