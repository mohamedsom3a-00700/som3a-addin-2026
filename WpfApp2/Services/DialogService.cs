using System;
using System.Threading.Tasks;
using System.Windows;

namespace Som3a_WPF_UI.Services
{
    public enum DialogType
    {
        Confirm,
        Warning,
        Error
    }

    public static class DialogService
    {
        public static Task<bool> ShowConfirmAsync(string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                tcs.SetResult(result == MessageBoxResult.Yes);
            });

            return tcs.Task;
        }

        public static Task ShowMessageAsync(string title, string message, DialogType type = DialogType.Confirm)
        {
            var tcs = new TaskCompletionSource<bool>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var icon = type switch
                {
                    DialogType.Warning => MessageBoxImage.Warning,
                    DialogType.Error => MessageBoxImage.Error,
                    _ => MessageBoxImage.Information
                };

                MessageBox.Show(message, title, MessageBoxButton.OK, icon);
                tcs.SetResult(true);
            });

            return tcs.Task;
        }

        public static async Task<bool> ConfirmAsync(string message)
        {
            return await ShowConfirmAsync("Confirm", message);
        }

        public static async Task WarnAsync(string message)
        {
            await ShowMessageAsync("Warning", message, DialogType.Warning);
        }

        public static async Task ErrorAsync(string message)
        {
            await ShowMessageAsync("Error", message, DialogType.Error);
        }
    }
}