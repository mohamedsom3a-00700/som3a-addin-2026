using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Views
{
    public partial class AIPanel : UserControl
    {
        public AIPanel()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Set the PasswordBox from the current API key on load
            if (DataContext is ViewModels.SettingsViewModel vm && !string.IsNullOrEmpty(vm.AICloudApiKey))
            {
                ApiKeyBox.Password = vm.AICloudApiKey;
            }
        }

        private void OnApiKeyChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.SettingsViewModel vm)
            {
                vm.AICloudApiKey = ApiKeyBox.Password;
            }
        }
    }
}
