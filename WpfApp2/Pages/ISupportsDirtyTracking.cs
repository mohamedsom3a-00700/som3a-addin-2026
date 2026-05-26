using System;

namespace Som3a_WPF_UI.Pages
{
    public interface ISupportsDirtyTracking
    {
        bool IsDirty { get; }
        event EventHandler<bool> DirtyChanged;
    }
}
