using System;
using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Pages
{
    public abstract class PageBase : Page, ISupportsDirtyTracking
    {
        private bool _isDirty;

        protected PageBase()
        {
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }

        public virtual bool IsDirty => _isDirty;

        public event EventHandler<bool> DirtyChanged;

        public void SetDirty(bool dirty)
        {
            if (_isDirty != dirty)
            {
                _isDirty = dirty;
                DirtyChanged?.Invoke(this, dirty);
            }
        }

        protected virtual void OnPageLoaded(object sender, RoutedEventArgs e)
        {
        }

        protected virtual void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
