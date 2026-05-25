using System.Windows;
using System.Windows.Media.Animation;

namespace Som3a_WPF_UI.Theme.Controls
{
    public static class AnimationProperties
    {
        public static readonly DependencyProperty AnimationScaleProperty =
            DependencyProperty.RegisterAttached(
                "AnimationScale",
                typeof(double),
                typeof(AnimationProperties),
                new PropertyMetadata(1.0, OnAnimationScaleChanged));

        public static double GetAnimationScale(DependencyObject obj)
        {
            return (double)obj.GetValue(AnimationScaleProperty);
        }

        public static void SetAnimationScale(DependencyObject obj, double value)
        {
            obj.SetValue(AnimationScaleProperty, value);
        }

        private static void OnAnimationScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Timeline timeline && e.NewValue is double scale)
            {
                timeline.Duration = new Duration(System.TimeSpan.FromTicks(
                    (long)(timeline.Duration.TimeSpan.Ticks * scale)));
            }
        }
    }
}
