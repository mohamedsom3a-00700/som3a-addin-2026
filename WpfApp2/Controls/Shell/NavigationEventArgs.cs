using System;

namespace Som3a_WPF_UI.Controls.Shell
{
    public class NavigationEventArgs : EventArgs
    {
        public string PreviousKey { get; set; }
        public string NewKey { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
