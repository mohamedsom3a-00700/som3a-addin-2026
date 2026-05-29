using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Som3a_WPF_UI.Services
{
    public class TranslationSource : INotifyPropertyChanged
    {
        private static readonly TranslationSource _instance = new();
        public static TranslationSource Instance => _instance;

        private TranslationSource() { }

        public string this[string key]
        {
            get
            {
                var val = LocalizationBridgeService.Instance.GetString(key);
                return string.IsNullOrEmpty(val) ? key : val;
            }
        }

        public void Refresh()
        {
            FirePropertyChanged("Item[]");
            FirePropertyChanged(string.Empty);
            ForceRefreshAllBindingExpressions();
        }

        private static void FirePropertyChanged(string name)
        {
            var source = _instance;
            if (source.PropertyChanged != null)
                source.PropertyChanged(source, new PropertyChangedEventArgs(name));
        }

        private static void ForceRefreshAllBindingExpressions()
        {
            for (int i = Application.Current.Windows.Count - 1; i >= 0; i--)
            {
                var window = Application.Current.Windows[i];
                if (window != null)
                    RefreshBindings(window);
            }
        }

        private static void RefreshBindings(DependencyObject element)
        {
            if (element == null) return;

            var localValues = element.GetLocalValueEnumerator();
            while (localValues.MoveNext())
            {
                var entry = localValues.Current;
                if (BindingOperations.IsDataBound(element, entry.Property))
                {
                    var expr = BindingOperations.GetBindingExpression(element, entry.Property);
                    if (expr != null)
                        expr.UpdateTarget();
                }
            }

            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                RefreshBindings(child);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
