using System.ComponentModel;
using System.Runtime.CompilerServices;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Controls.Shell
{
    public class NavigationDestination : INotifyPropertyChanged
    {
        private string _key;
        private string _label;
        private string _icon;
        private int _order;
        private bool _isVisible = true;
        private bool _isSelected;
        private string _category;
        private string _itemId;
        private bool _isEnabled = true;
        private int _priority;
        private string _resourceKey;

        public string Key
        {
            get => _key;
            set { _key = value; OnPropertyChanged(); }
        }

        public string Label
        {
            get
            {
                if (_resourceKey != null)
                {
                    var translated = LocalizationBridgeService.Instance.GetString(_resourceKey);
                    if (!string.IsNullOrEmpty(translated) && translated != _resourceKey)
                        return translated;
                }
                return _label;
            }
            set { _label = value; OnPropertyChanged(); }
        }

        public string ResourceKey
        {
            get => _resourceKey;
            set
            {
                _resourceKey = value;
                OnPropertyChanged(nameof(Label));
            }
        }

        public string Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(); }
        }

        public int Order
        {
            get => _order;
            set { _order = value; OnPropertyChanged(); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public string ItemId
        {
            get => _itemId;
            set { _itemId = value; OnPropertyChanged(); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        public int Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(); }
        }

        public void RefreshLabel()
        {
            OnPropertyChanged(nameof(Label));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
