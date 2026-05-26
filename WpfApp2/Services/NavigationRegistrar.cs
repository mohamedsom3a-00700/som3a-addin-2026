using System;
using System.Collections.Generic;
using Som3a_WPF_UI.Contracts;

namespace Som3a_WPF_UI.Services
{
    public class NavigationRegistrar : INavigationRegistrar
    {
        private readonly List<(string Id, string Title, Type PageType, string Category)> _pages = new();

        public IReadOnlyList<(string Id, string Title, Type PageType, string Category)> RegisteredPages => _pages.AsReadOnly();

        public void RegisterPage(string id, string title, Type pageType)
        {
            RegisterPage(id, title, pageType, null);
        }

        public void RegisterPage(string id, string title, Type pageType, string category)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));
            if (pageType is null)
                throw new ArgumentNullException(nameof(pageType));
            if (!typeof(System.Windows.Controls.Page).IsAssignableFrom(pageType))
                throw new ArgumentException($"Type '{pageType.FullName}' must derive from System.Windows.Controls.Page.", nameof(pageType));

            _pages.Add((id, title, pageType, category ?? "Other"));
        }
    }
}
