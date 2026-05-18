using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Som3a_WPF_UI.Controls
{
    public partial class LoadingOverlay : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(LoadingOverlay),
                new PropertyMetadata(string.Empty, OnMessageChanged));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public LoadingOverlay()
        {
            InitializeComponent();
            Loaded += (s, e) => StartAnimation();
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingOverlay overlay)
            {
                overlay.MessageText.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private void StartAnimation()
        {
            var storyboard = (Storyboard)Resources["SpinAnimation"];
            storyboard.Begin(this, true);
        }
    }
}