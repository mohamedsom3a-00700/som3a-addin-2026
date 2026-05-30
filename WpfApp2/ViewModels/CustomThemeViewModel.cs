using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels
{
    public partial class CustomThemeViewModel : ViewModelBase
    {
        private readonly IServiceContainer _container;

        [ObservableProperty]
        private string _backgroundType = "Solid";

        [ObservableProperty]
        private string _imagePath = "";

        partial void OnImagePathChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                ImageFileName = System.IO.Path.GetFileName(value);
            }
            else
            {
                ImageFileName = "";
            }
        }

        [ObservableProperty]
        private string _imageFileName = "";

        private double _blurIntensity;
        public double BlurIntensity
        {
            get => _blurIntensity;
            set
            {
                if (SetProperty(ref _blurIntensity, Math.Max(0.0, Math.Min(1.0, value))))
                {
                    OnPropertyChanged(nameof(BlurIntensityPercent));
                    if (BlurEnabled)
                        _container.Resolve<ThemeManager>().ApplyBackground(ImagePath, _blurIntensity);
                }
            }
        }

        public double BlurIntensityPercent
        {
            get => BlurIntensity * 100.0;
            set => BlurIntensity = value / 100.0;
        }

        [ObservableProperty]
        private bool _blurEnabled;

        partial void OnBlurEnabledChanged(bool value)
        {
            _container.Resolve<ThemeManager>().ApplyBackground(ImagePath, value ? BlurIntensity : 0.0);
        }

        [ObservableProperty]
        private string _selectedFontFamily = "Segoe UI";

        partial void OnSelectedFontFamilyChanged(string value)
        {
            _container.Resolve<ThemeManager>().ApplyFont(value);
        }

        [ObservableProperty]
        private FontFamilyInfo _selectedFont;

        partial void OnSelectedFontChanged(FontFamilyInfo value)
        {
            if (value != null)
            {
                SelectedFontFamily = value.FamilyName;
            }
        }

        public ObservableCollection<FontFamilyInfo> AvailableFonts { get; }

        [ObservableProperty]
        private string _imageValidationError = "";

        public CustomThemeViewModel(IServiceContainer container)
        {
            _container = container;
            AvailableFonts = new ObservableCollection<FontFamilyInfo>();
        }

        [RelayCommand]
        private void SelectImage()
        {
            var themeManager = _container.Resolve<ThemeManager>();
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*",
                Title = "Select Background Image"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var fileInfo = new System.IO.FileInfo(dlg.FileName);
                    if (fileInfo.Length > 10 * 1024 * 1024)
                    {
                        ImageValidationError = "Image must be 10MB or smaller";
                        return;
                    }

                    var img = new System.Windows.Media.Imaging.BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(dlg.FileName);
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.EndInit();

                    if (img.PixelWidth > 4096 || img.PixelHeight > 4096)
                    {
                        ImageValidationError = "Image dimensions must be 4096px or smaller";
                        return;
                    }

                    ImagePath = dlg.FileName;
                    ImageValidationError = "";
                    BackgroundType = "Image";
                    themeManager.ApplyBackground(ImagePath, BlurEnabled ? BlurIntensity : 0.0);
                }
                catch
                {
                    ImageValidationError = "Failed to load image. File may be corrupt.";
                }
            }
        }

        [RelayCommand]
        private void ClearImage()
        {
            var themeManager = _container.Resolve<ThemeManager>();
            ImagePath = "";
            ImageFileName = "";
            BackgroundType = "Solid";
            ImageValidationError = "";
            themeManager.ApplyBackground("", 0.0);
        }

        [RelayCommand]
        private void ApplyFont()
        {
            if (SelectedFont != null)
            {
                var themeManager = _container.Resolve<ThemeManager>();
                themeManager.ApplyFont(SelectedFont.FamilyName);
            }
        }

        public void LoadFonts()
        {
            AvailableFonts.Clear();
            var fonts = FontEnumerator.GetSystemFonts();
            foreach (var font in fonts)
            {
                AvailableFonts.Add(font);
            }
        }
    }
}
