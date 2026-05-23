using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Som3a_WPF_UI.Controls.Shell;

namespace Som3a_WPF_UI.Services
{
    public interface INavigationService
    {
        void RegisterPage<T>(string key, string displayName, string icon = null, int order = 50) where T : Page, new();
        void NavigateTo(string key, bool pushToHistory = true, string overridePreviousKey = null);
        bool GoBack();
        Page CreatePage(string key);
        bool IsPageRegistered(string key);
        List<NavigationDestination> Search(string query);
        ObservableCollection<NavigationDestination> Destinations { get; }
        event EventHandler<NavigationEventArgs> NavigationChanged;
    }

    public class NavigationService : INavigationService
    {
        private static readonly Lazy<NavigationService> _instance =
            new Lazy<NavigationService>(() => new NavigationService());

        public static NavigationService Instance => _instance.Value;

        private readonly Dictionary<string, NavigationPage> _registry =
            new Dictionary<string, NavigationPage>();

        private readonly Stack<string> _navigationHistory = new Stack<string>();

        private ShellWindow _shellWindow;

        public ObservableCollection<NavigationDestination> Destinations { get; } =
            new ObservableCollection<NavigationDestination>();

        public event EventHandler<NavigationEventArgs> NavigationChanged;

        protected NavigationService()
        {
        }

        public void RegisterPage<T>(string key, string displayName, string icon = null, int order = 50)
            where T : Page, new()
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must not be null or empty", nameof(key));

            if (displayName == null)
                throw new ArgumentException("Display name must not be null", nameof(displayName));

            if (_registry.ContainsKey(key))
                throw new InvalidOperationException($"A page with key '{key}' is already registered.");

            var navPage = new NavigationPage
            {
                Key = key,
                DisplayName = displayName,
                Icon = icon,
                Order = order,
                PageType = typeof(T)
            };

            _registry[key] = navPage;

            var destination = new NavigationDestination
            {
                Key = key,
                Label = displayName,
                Icon = icon,
                Order = order,
                IsVisible = true
            };

            var insertIndex = 0;
            while (insertIndex < Destinations.Count &&
                   Destinations[insertIndex].Order <= order)
                insertIndex++;

            Destinations.Insert(insertIndex, destination);
        }

        public void NavigateTo(string key, bool pushToHistory = true, string overridePreviousKey = null)
        {
            if (!_registry.TryGetValue(key, out var navPage))
                throw new KeyNotFoundException($"No page registered with key '{key}'.");

            EnsureShellOpen();

            var previousKey = _navigationHistory.Count > 0 ? _navigationHistory.Peek() : null;
            if (pushToHistory && (previousKey == null || !previousKey.Equals(key)))
                _navigationHistory.Push(key);

            var destination = Destinations.FirstOrDefault(d => d.Key == key);
            if (destination != null)
            {
                _shellWindow.NavigateToDestination(destination);
            }

            OnNavigationChanged(new NavigationEventArgs
            {
                PreviousKey = overridePreviousKey ?? previousKey,
                NewKey = key,
                Success = true
            });
        }

        public bool GoBack()
        {
            if (_navigationHistory.Count <= 1)
                return false;

            var poppedKey = _navigationHistory.Pop();
            var targetKey = _navigationHistory.Peek();

            NavigateTo(targetKey, pushToHistory: false, overridePreviousKey: poppedKey);
            return true;
        }

        public Page CreatePage(string key)
        {
            if (_registry.TryGetValue(key, out var navPage))
                return navPage.CreateInstance();
            return null;
        }

        public bool IsPageRegistered(string key)
        {
            return _registry.ContainsKey(key);
        }

        public List<NavigationDestination> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Destinations.ToList();

            var lowerQuery = query.ToLowerInvariant();
            return Destinations
                .Where(d => (d.Label ?? string.Empty).ToLowerInvariant().Contains(lowerQuery))
                .OrderBy(d => d.Order)
                .ToList();
        }

        private void EnsureShellOpen()
        {
            if (_shellWindow == null || !_shellWindow.IsLoaded)
            {
                _shellWindow = new ShellWindow();
                _shellWindow.Closed += (s, e) => _shellWindow = null;
                _shellWindow.Show();
            }
            else
            {
                _shellWindow.Activate();
            }
        }

        public void SetShellWindow(ShellWindow window)
        {
            _shellWindow = window;
        }

        protected virtual void OnNavigationChanged(NavigationEventArgs e)
        {
            NavigationChanged?.Invoke(this, e);
        }
    }
}
