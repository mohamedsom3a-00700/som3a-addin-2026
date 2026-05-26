using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Som3a_WPF_UI.Controls
{
    public class FontPreview : Control
    {
        static FontPreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontPreview),
                new FrameworkPropertyMetadata(typeof(FontPreview)));
        }

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register(
                nameof(FontFamily),
                typeof(FontFamily),
                typeof(FontPreview),
                new PropertyMetadata(new FontFamily("Segoe UI")));

        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public static readonly DependencyProperty FontNameProperty =
            DependencyProperty.Register(
                nameof(FontName),
                typeof(string),
                typeof(FontPreview),
                new PropertyMetadata("Segoe UI"));

        public string FontName
        {
            get => (string)GetValue(FontNameProperty);
            set => SetValue(FontNameProperty, value);
        }

        public static readonly DependencyProperty PreviewTextProperty =
            DependencyProperty.Register(
                nameof(PreviewText),
                typeof(string),
                typeof(FontPreview),
                new PropertyMetadata("AaBbCcDdEeFfGg"));

        public string PreviewText
        {
            get => (string)GetValue(PreviewTextProperty);
            set => SetValue(PreviewTextProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(FontPreview),
                new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
    }
}
