using System;
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

    public class GridLengthAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(GridLength);

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(GridLength?), typeof(GridLengthAnimation));

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(GridLength?), typeof(GridLengthAnimation));

        public GridLength? From
        {
            get => (GridLength?)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public GridLength? To
        {
            get => (GridLength?)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock.CurrentProgress == null)
                return new GridLength(220);

            var from = From ?? (defaultOriginValue is GridLength gl ? gl : new GridLength(220));
            var to = To ?? (defaultDestinationValue is GridLength gl2 ? gl2 : new GridLength(48));
            var progress = animationClock.CurrentProgress.Value;

            var fromValue = from.IsAbsolute ? from.Value : 220;
            var toValue = to.IsAbsolute ? to.Value : 48;

            var currentValue = fromValue + (toValue - fromValue) * progress;
            return new GridLength(currentValue);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }
    }
}
