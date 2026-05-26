using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels
{
    public class CustomThemeViewModel : ViewModelBase
    {
        private readonly IServiceContainer _container;
        private string _backgroundType = "Solid";
        private string _imagePath = "";
        private double _blurIntensity = 0.0;
        private bool _blurEnabled = false;
        private string _selectedFontFamily = "Segoe UI";
        private FontFamilyInfo _selectedFont;
        private string _imageValidationError = "";

        public CustomThemeViewModel(IServiceContainer container)
        {
            _container = container;
            AvailableFonts = new ObservableCollection<FontFamilyInfo>();
            SelectImageCommand = new RelayCommand(ExecuteSelectImage);
            ClearImageCommand = new RelayCommand(ExecuteClearImage);
            ApplyFontCommand = new RelayCommand(ExecuteApplyFont);
        }

        public string BackgroundType
        {
            get => _backgroundType;
            set => SetProperty(ref _backgroundType, value);
        }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (SetProperty(ref _imagePath, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var fileName = System.IO.Path.GetFileName(value);
                        ImageFileName = fileName;
                    }
                    else
                    {
                        ImageFileName = "";
                    }
                }
            }
        }

        private string _imageFileName = "";
        public string ImageFileName
        {
            get => _imageFileName;
            set => SetProperty(ref _imageFileName, value);
        }

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
            get => _blurIntensity * 100.0;
            set => BlurIntensity = value / 100.0;
        }

        public bool BlurEnabled
        {
            get => _blurEnabled;
            set
            {
                if (SetProperty(ref _blurEnabled, value))
                {
                    _container.Resolve<ThemeManager>().ApplyBackground(ImagePath, _blurEnabled ? _blurIntensity : 0.0);
                }
            }
        }

        public string SelectedFontFamily
        {
            get => _selectedFontFamily;
            set
            {
                if (SetProperty(ref _selectedFontFamily, value))
                {
                    _container.Resolve<ThemeManager>().ApplyFont(value);
                }
            }
        }

        public FontFamilyInfo SelectedFont
        {
            get => _selectedFont;
            set
            {
                if (SetProperty(ref _selectedFont, value) && value != null)
                {
                    SelectedFontFamily = value.FamilyName;
                }
            }
        }

        public ObservableCollection<FontFamilyInfo> AvailableFonts { get; }

        public string ImageValidationError
        {
            get => _imageValidationError;
            set => SetProperty(ref _imageValidationError, value);
        }

        public ICommand SelectImageCommand { get; }
        public ICommand ClearImageCommand { get; }
        public ICommand ApplyFontCommand { get; }

        private void ExecuteSelectImage()
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

        private void ExecuteClearImage()
        {
            var themeManager = _container.Resolve<ThemeManager>();
            ImagePath = "";
            ImageFileName = "";
            BackgroundType = "Solid";
            ImageValidationError = "";
            themeManager.ApplyBackground("", 0.0);
        }

        private void ExecuteApplyFont()
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
