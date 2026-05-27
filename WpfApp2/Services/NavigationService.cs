using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Som3a_WPF_UI.Controls.Shell;
using Som3a_WPF_UI.Pages;

namespace Som3a_WPF_UI.Services
{
    public interface INavigationService
    {
        void RegisterPage<T>(string key, string displayName, string icon = null, int order = 50) where T : Page, new();
        void RegisterPage(Type pageType, string key, string displayName, string icon = null, int order = 50);
        void RegisterPage(string category, Type pageType, string key, string displayName, string icon = null, int order = 50);
        void NavigateTo(string key, bool pushToHistory = true, string overridePreviousKey = null);
        bool GoBack();
        Page CreatePage(string key);
        bool IsPageRegistered(string key);
        List<NavigationDestination> Search(string query);
        IReadOnlyList<string> GetCategories();
        NavigationDestination GetActiveDestination();
        bool RequestNavigation(string targetKey);
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

        private static readonly Dictionary<string, int> CategoryOrder = new Dictionary<string, int>
        {
            { "Planning", 1 },
            { "Analysis", 2 },
            { "Excel", 3 },
            { "AI", 4 },
            { "Settings", 5 },
            { "Other", 99 }
        };

        public Dictionary<string, object> NavigationData { get; } =
            new Dictionary<string, object>();

        public ObservableCollection<NavigationDestination> Destinations { get; } =
            new ObservableCollection<NavigationDestination>();

        public event EventHandler<NavigationEventArgs> NavigationChanged;

        protected NavigationService()
        {
        }

        public void RegisterPage<T>(string key, string displayName, string icon = null, int order = 50)
            where T : Page, new()
            => RegisterPageByType(typeof(T), key, displayName, icon, order, null);

        public void RegisterPage(Type pageType, string key, string displayName, string icon = null, int order = 50)
            => RegisterPageByType(pageType, key, displayName, icon, order, null);

        public void RegisterPage(string category, Type pageType, string key, string displayName, string icon = null, int order = 50)
            => RegisterPageByType(pageType, key, displayName, icon, order, category ?? "Other");

        private void RegisterPageByType(Type pageType, string key, string displayName, string icon, int order, string category)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must not be null or empty", nameof(key));
            if (displayName == null)
                throw new ArgumentException("Display name must not be null", nameof(displayName));
            if (pageType == null)
                throw new ArgumentNullException(nameof(pageType));
            if (!typeof(System.Windows.Controls.Page).IsAssignableFrom(pageType))
                throw new ArgumentException($"Type '{pageType.FullName}' must derive from System.Windows.Controls.Page", nameof(pageType));

            if (_registry.ContainsKey(key))
                throw new InvalidOperationException($"A page with key '{key}' is already registered.");

            var resolvedCategory = string.IsNullOrEmpty(category) ? "Other" : category;
            if (!CategoryOrder.ContainsKey(resolvedCategory))
                resolvedCategory = "Other";

            var navPage = new NavigationPage
            {
                Key = key,
                DisplayName = displayName,
                Icon = icon,
                Order = order,
                PageType = pageType,
                Category = resolvedCategory
            };

            _registry[key] = navPage;

            var destination = new NavigationDestination
            {
                Key = key,
                Label = displayName,
                Icon = icon,
                Order = order,
                IsVisible = true,
                IsEnabled = true,
                Category = resolvedCategory,
                ItemId = key
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

            try
            {
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigationService.NavigateTo failed for '{key}': {ex.Message}");
                OnNavigationChanged(new NavigationEventArgs
                {
                    PreviousKey = overridePreviousKey ?? previousKey,
                    NewKey = key,
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        public bool GoBack()
        {
            if (_navigationHistory.Count <= 1)
                return false;

            var poppedKey = _navigationHistory.Pop();
            var targetKey = _navigationHistory.Peek();

            try
            {
                NavigateTo(targetKey, pushToHistory: false, overridePreviousKey: poppedKey);
            }
            catch
            {
                _navigationHistory.Push(poppedKey);
                throw;
            }
            return true;
        }

        public Page CreatePage(string key)
        {
            try
            {
                if (_registry.TryGetValue(key, out var navPage))
                    return navPage.CreateInstance();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigationService.CreatePage failed for '{key}': {ex.Message}");
            }
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

        public IReadOnlyList<string> GetCategories()
        {
            return Destinations
                .Select(d => d.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => CategoryOrder.TryGetValue(c, out var order) ? order : 99)
                .ToList()
                .AsReadOnly();
        }

        public NavigationDestination GetActiveDestination()
        {
            return Destinations.FirstOrDefault(d => d.IsSelected);
        }

        public bool RequestNavigation(string targetKey)
        {
            if (_shellWindow == null)
                return false;

            if (!_registry.ContainsKey(targetKey))
            {
                OnNavigationChanged(new NavigationEventArgs
                {
                    NewKey = targetKey,
                    Success = false,
                    Cancelled = false
                });
                return false;
            }

            if (_shellWindow.Workspace?.CurrentPage is ISupportsDirtyTracking dirtyPage && dirtyPage.IsDirty)
            {
                var result = System.Windows.MessageBox.Show(
                    "You have unsaved changes. Save before navigating?",
                    "Unsaved Changes",
                    System.Windows.MessageBoxButton.YesNoCancel,
                    System.Windows.MessageBoxImage.Warning);

                switch (result)
                {
                    case System.Windows.MessageBoxResult.Yes:
                        SaveCurrentPage();
                        break;
                    case System.Windows.MessageBoxResult.No:
                        break;
                    case System.Windows.MessageBoxResult.Cancel:
                        OnNavigationChanged(new NavigationEventArgs
                        {
                            NewKey = targetKey,
                            Success = false,
                            Cancelled = true
                        });
                        return false;
                }
            }

            NavigateTo(targetKey);
            return true;
        }

        private void SaveCurrentPage()
        {
            if (_shellWindow?.Workspace?.CurrentPage is PageBase dirtyPage && dirtyPage.IsDirty)
            {
                dirtyPage.SetDirty(false);
            }
        }

        private void EnsureShellOpen()
        {
            if (_shellWindow == null || !_shellWindow.IsVisible)
            {
                if (_shellWindow != null && !_shellWindow.IsVisible)
                {
                    _shellWindow.Close();
                    _shellWindow = null;
                }
                _shellWindow = new ShellWindow();
                _shellWindow.Closed += (s, e) => _shellWindow = null;
                _shellWindow.Show();
            }
            else
            {
                _shellWindow.Activate();
            }
        }

        public ShellWindow ShellWindow => _shellWindow;

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
