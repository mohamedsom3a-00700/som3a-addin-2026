using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Som3a.Shared.Models
{
    public sealed class LinkItem : INotifyPropertyChanged
    {
        public string LinkSource { get; set; } = "";
        public string SheetOrObject { get; set; } = "";
        public string CellOrDetail { get; set; } = "";
        public string Type { get; set; } = "";
        public string? TagLink { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _status = "Pending";
        public string Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(); } }
        }

        private bool _isBreaking;
        public bool IsBreaking
        {
            get => _isBreaking;
            set { if (_isBreaking != value) { _isBreaking = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

}
