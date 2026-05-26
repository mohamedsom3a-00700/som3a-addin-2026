using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Som3a_WPF_UI.Controls
{
    public class ColorWheel : FrameworkElement
    {
        private WriteableBitmap _bitmap;
        private bool _isDragging;

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                nameof(SelectedColor),
                typeof(Color),
                typeof(ColorWheel),
                new FrameworkPropertyMetadata(
                    Color.FromRgb(58, 134, 255),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnSelectedColorChanged));

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public static readonly DependencyProperty WheelRadiusProperty =
            DependencyProperty.Register(
                nameof(WheelRadius),
                typeof(double),
                typeof(ColorWheel),
                new PropertyMetadata(128.0));

        public double WheelRadius
        {
            get => (double)GetValue(WheelRadiusProperty);
            set => SetValue(WheelRadiusProperty, value);
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wheel = (ColorWheel)d;
            wheel.InvalidateVisual();
        }

        static ColorWheel()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(ColorWheel), new FrameworkPropertyMetadata(true));
        }

        public ColorWheel()
        {
            Width = 256;
            Height = 256;
            Cursor = Cursors.Cross;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GenerateWheelBitmap();
        }

        private void GenerateWheelBitmap()
        {
            int size = 256;
            int cx = size / 2;
            int cy = size / 2;
            double radius = size / 2.0;

            _bitmap = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new byte[size * size * 4];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    int idx = (y * size + x) * 4;
                    pixels[idx + 3] = 255;

                    if (distance <= radius)
                    {
                        double hue = Math.Atan2(dy, dx) * 180.0 / Math.PI + 180.0;
                        double saturation = Math.Min(1.0, distance / radius);

                        var rgb = HsvToRgb(hue / 360.0, saturation, 1.0);
                        pixels[idx + 2] = rgb.R;
                        pixels[idx + 1] = rgb.G;
                        pixels[idx + 0] = rgb.B;
                    }
                    else
                    {
                        pixels[idx + 2] = 0;
                        pixels[idx + 1] = 0;
                        pixels[idx + 0] = 0;
                    }
                }
            }

            _bitmap.WritePixels(new Int32Rect(0, 0, size, size), pixels, size * 4, 0);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (_bitmap != null)
            {
                dc.DrawImage(_bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
            }

            double cx = ActualWidth / 2;
            double cy = ActualHeight / 2;

            dc.DrawEllipse(
                new SolidColorBrush(SelectedColor),
                new Pen(Brushes.White, 2),
                new Point(cx, cy), 5, 5);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _isDragging = true;
            CaptureMouse();
            UpdateColorFromPoint(e.GetPosition(this));
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging)
            {
                UpdateColorFromPoint(e.GetPosition(this));
                e.Handled = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            e.Handled = true;
        }

        private void UpdateColorFromPoint(Point point)
        {
            double cx = ActualWidth / 2;
            double cy = ActualHeight / 2;
            double radius = Math.Min(cx, cy);
            double dx = point.X - cx;
            double dy = point.Y - cy;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= radius)
            {
                double hue = Math.Atan2(dy, dx) * 180.0 / Math.PI + 180.0;
                double saturation = Math.Min(1.0, distance / radius);
                SelectedColor = HsvToRgb(hue / 360.0, saturation, 1.0);
            }
        }

        private static Color HsvToRgb(double h, double s, double v)
        {
            h = Math.Max(0, Math.Min(1, h)) * 6;
            int sector = (int)h;
            double frac = h - sector;
            double p = v * (1 - s);
            double q = v * (1 - s * frac);
            double t = v * (1 - s * (1 - frac));

            double r, g, b;
            switch (sector % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            return Color.FromRgb(
                (byte)Math.Round(r * 255),
                (byte)Math.Round(g * 255),
                (byte)Math.Round(b * 255));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double size = WheelRadius * 2;
            return new Size(size, size);
        }
    }
}
