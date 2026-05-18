using Som3a.Shared.Core;
using Som3a.Shared.Models;
using Som3a_WPF_UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Som3a_WPF_UI.Controls;
namespace Som3a_WPF_UI
{
    public partial class Float_path : ModernWindow
    {
        private FloatPathViewModel vm;
        private void WebMessageHandler(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var id = e.TryGetWebMessageAsString();

            MessageBox.Show($"Clicked Activity ID: {id}");

            if (DataContext is FloatPathViewModel vm)
            {
                vm.SelectActivityById(id);
            }
        }
        private string _pendingHtml;
        public Float_path()
        {
            InitializeComponent();

            vm = new FloatPathViewModel();
            DataContext = vm;

            Loaded += async (s, e) =>
            {
                try
                {
                    MessageBox.Show("Loaded Fired ✅");

                    var userDataFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Som3aWebView"
                    );

                    var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                        null,
                        userDataFolder
                    );

                    await GraphBrowser.EnsureCoreWebView2Async(env);

                    MessageBox.Show("WebView Ready ✅");

                    // 🔥 أهم إضافة
                    if (!string.IsNullOrEmpty(_pendingHtml))
                    {
                        GraphBrowser.CoreWebView2.NavigateToString(_pendingHtml);
                        _pendingHtml = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR: " + ex.Message);
                }
            };

            vm.SendGraphToUI = (html) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (GraphBrowser.CoreWebView2 != null)
                    {
                        GraphBrowser.CoreWebView2.NavigateToString(html);
                    }
                    else
                    {
                        // 🔥 خزّن الـ HTML مؤقتًا
                        _pendingHtml = html;
                    }
                });
            };
        }
        private void Activity_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is FloatPathViewModel vm)
            {
                var activity = (sender as TextBlock)?.DataContext as Activity;
                vm.SelectedActivity = activity;
            }
        }
        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is FloatPathViewModel vm)
            {
                if (e.NewValue is Activity act)
                {
                    vm.SelectedActivity = act;
                }
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnMin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}