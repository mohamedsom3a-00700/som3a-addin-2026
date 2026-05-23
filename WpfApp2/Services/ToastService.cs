using System;
using System.Windows;
using Som3a_WPF_UI.Controls.Toast;
using Som3a_WPF_UI.ViewModels;

namespace Som3a_WPF_UI.Services
{
    public static class ToastService
    {
        public static void Show(string message, ToastType type = ToastType.Info, int durationMs = 3000)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var viewModel = new ToastViewModel(new ToastModel
                {
                    Message = message,
                    Type = type,
                    DurationMs = durationMs
                });
                var toast = new ToastWindow(viewModel);
                toast.Show();
            });
        }

        public static void Success(string message) => Show(message, ToastType.Success);
        public static void Error(string message) => Show(message, ToastType.Error);
        public static void Warning(string message) => Show(message, ToastType.Warning);
        public static void Info(string message) => Show(message, ToastType.Info);
    }
}
