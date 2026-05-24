using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Som3a_WPF_UI.Controls.Toast;

namespace Som3a_WPF_UI.ViewModels
{
    public sealed class ToastViewModel : ViewModelBase
    {
        private static readonly Dictionary<ToastType, (string Icon, string BackgroundKey)> TypeMap = new()
        {
            [ToastType.Success] = ("✓", "Brush.Accent.Success"),
            [ToastType.Error] = ("✗", "Brush.Accent.Danger"),
            [ToastType.Warning] = ("⚠", "Brush.Accent.Warning"),
            [ToastType.Info] = ("ℹ", "Brush.Accent.Primary"),
        };

        public ToastType ToastType { get; }
        public string Message { get; }
        public int DurationMs { get; }
        public string Icon { get; }
        public Brush Background { get; }
        public Brush Foreground { get; }

        public ToastViewModel(ToastModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));
            ToastType = model.Type;
            Message = model.Message;
            DurationMs = model.DurationMs;

            var config = TypeMap[model.Type];
            Icon = config.Icon;
            Background = (Brush)Application.Current.FindResource(config.BackgroundKey);
            Foreground = (Brush)Application.Current.FindResource("Brush.Text.OnAccent");
        }
    }
}
