using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Som3a_WPF_UI.Controls.Shell
{
    public class ShellState : INotifyPropertyChanged
    {
        private string _activePageKey;
        private string _lastActivePageKey;
        private bool _sidebarVisible = true;
        private bool _sidebarCollapsed;
        private bool _commandPaletteOpen;
        private readonly HashSet<string> _collapsedCategories = new HashSet<string>();
        private readonly Stack<string> _previousPageStack = new Stack<string>();

        public const int MaxHistoryDepth = 10;

        public string ActivePageKey
        {
            get => _activePageKey;
            set
            {
                if (_activePageKey != value)
                {
                    _lastActivePageKey = _activePageKey;
                    _activePageKey = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LastActivePageKey));
                }
            }
        }

        public string LastActivePageKey
        {
            get => _lastActivePageKey;
            private set { _lastActivePageKey = value; OnPropertyChanged(); }
        }

        public bool SidebarVisible
        {
            get => _sidebarVisible;
            set { _sidebarVisible = value; OnPropertyChanged(); }
        }

        public bool SidebarCollapsed
        {
            get => _sidebarCollapsed;
            set
            {
                if (_sidebarCollapsed != value)
                {
                    _sidebarCollapsed = value;
                    OnPropertyChanged();
                }
            }
        }

        public HashSet<string> CollapsedCategories => _collapsedCategories;

        public bool CommandPaletteOpen
        {
            get => _commandPaletteOpen;
            set { _commandPaletteOpen = value; OnPropertyChanged(); }
        }

        public Stack<string> PreviousPageStack => _previousPageStack;

        public void PushPage(string key)
        {
            if (_previousPageStack.Count >= MaxHistoryDepth)
            {
                var temp = _previousPageStack.ToArray();
                _previousPageStack.Clear();
                for (int i = temp.Length - 2; i >= 0; i--)
                    _previousPageStack.Push(temp[i]);
            }
            _previousPageStack.Push(key);
        }

        public string PopPage()
        {
            return _previousPageStack.Count > 0 ? _previousPageStack.Pop() : null;
        }

        public void ClearHistory()
        {
            _previousPageStack.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
