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

        private static readonly DependencyProperty OriginalDurationProperty =
            DependencyProperty.RegisterAttached(
                "OriginalDuration",
                typeof(Duration?),
                typeof(AnimationProperties),
                new PropertyMetadata(null));

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
            if (d is not Timeline timeline || e.NewValue is not double scale)
                return;

            if (!timeline.Duration.HasTimeSpan)
                return;

            var original = (Duration?)timeline.GetValue(OriginalDurationProperty);
            if (original == null || !original.Value.HasTimeSpan)
            {
                original = timeline.Duration;
                timeline.SetValue(OriginalDurationProperty, original);
            }

            timeline.Duration = new Duration(
                System.TimeSpan.FromTicks((long)(original.Value.TimeSpan.Ticks * scale)));
        }
    }
}
