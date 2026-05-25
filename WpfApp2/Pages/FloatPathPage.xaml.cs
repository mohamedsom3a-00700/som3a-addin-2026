using Som3a.Shared.Models;
using Som3a_WPF_UI.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Som3a_WPF_UI.Pages
{
    public partial class FloatPathPage : Page
    {
        private FloatPathViewModel vm;
        private string _pendingHtml;

        private void WebMessageHandler(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var id = e.TryGetWebMessageAsString();
            if (DataContext is FloatPathViewModel vm)
            {
                vm.SelectActivityById(id);
            }
        }

        public FloatPathPage()
        {
            InitializeComponent();

            vm = App.Container.Resolve<FloatPathViewModel>();
            DataContext = vm;

            Loaded += async (s, e) =>
            {
                try
                {
                    var userDataFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Som3aWebView"
                    );

                    var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                        null,
                        userDataFolder
                    );

                    await GraphBrowser.EnsureCoreWebView2Async(env);

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
                        _pendingHtml = html;
                    }
                });
            };
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
    }
}
