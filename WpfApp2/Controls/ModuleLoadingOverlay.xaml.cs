using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Som3a_WPF_UI.Controls
{
    public partial class ModuleLoadingOverlay : UserControl
    {
        public static readonly DependencyProperty ModuleNameProperty =
            DependencyProperty.Register(nameof(ModuleName), typeof(string), typeof(ModuleLoadingOverlay),
                new PropertyMetadata(string.Empty, OnModuleNameChanged));

        public string ModuleName
        {
            get => (string)GetValue(ModuleNameProperty);
            set => SetValue(ModuleNameProperty, value);
        }

        public ModuleLoadingOverlay()
        {
            InitializeComponent();
            Loaded += (s, e) => StartAnimation();
        }

        private static void OnModuleNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ModuleLoadingOverlay overlay)
                overlay.ModuleNameText.Text = e.NewValue as string ?? string.Empty;
        }

        private void StartAnimation()
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(new System.TimeSpan(0, 0, 0, 1)),
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(anim, SpinnerTransform);
            Storyboard.SetTargetProperty(anim, new PropertyPath("Angle"));
            sb.Children.Add(anim);
            sb.Begin(this, true);
        }
    }
}
