using Som3a.Shared.Core;
using Som3a_WPF_UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class WbsStyleSelectorViewModel : ViewModelBase
    {
        private int _selectedStyleId = 1;

        public int SelectedStyleId
        {
            get => _selectedStyleId;
            set
            {
                if (SetProperty(ref _selectedStyleId, value))
                    GeneratePreview();
            }
        }

        public ObservableCollection<StyleOption> StyleOptions { get; } = new();
        public ObservableCollection<StylePreviewItem> StylePreviews { get; } = new();

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action? CloseWindow { get; set; }

        public WbsStyleSelectorViewModel(IServiceContainer container)
        {
            SaveCommand = new RelayCommand(OnSave);
            CancelCommand = new RelayCommand(() => CloseWindow?.Invoke());

            InitializeOptions();
            _selectedStyleId = UserSettings.SelectedStyle;
            OnPropertyChanged(nameof(SelectedStyleId));
            GeneratePreview();
        }

        private void InitializeOptions()
        {
            StyleOptions.Add(new StyleOption(1, "Default"));
            StyleOptions.Add(new StyleOption(2, "Blue Gradient"));
            StyleOptions.Add(new StyleOption(3, "Primavera"));
            StyleOptions.Add(new StyleOption(4, "Dark Mode"));
            StyleOptions.Add(new StyleOption(5, "Soft Pastel"));
        }

        private void GeneratePreview()
        {
            StylePreviews.Clear();
            var style = WbsStyleFactory.GetStyle(_selectedStyleId);
            foreach (var kv in style)
            {
                var c = kv.Value.Fill;
                StylePreviews.Add(new StylePreviewItem
                {
                    Level = kv.Key,
                    Hex = $"#{c.R:X2}{c.G:X2}{c.B:X2}",
                    Brush = new SolidColorBrush(Color.FromRgb(c.R, c.G, c.B))
                });
            }
        }

        private void OnSave()
        {
            UserSettings.SelectedStyle = _selectedStyleId;
            CloseWindow?.Invoke();
        }
    }

    public sealed class StylePreviewItem
    {
        public int Level { get; set; }
        public string Hex { get; set; } = "";
        public Brush Brush { get; set; } = Brushes.Transparent;
    }

    public sealed class StyleOption
    {
        public int Id { get; }
        public string Name { get; }

        public StyleOption(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
