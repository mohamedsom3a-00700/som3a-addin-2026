using System.Windows.Controls;
using Som3a_WPF_UI.Contracts;

namespace WpfApp2.Modules.Sample
{
    public class SampleModule : IModule
    {
        public string Id => "som3a.sample";
        public string Version => "1.0.0";
        public string DisplayName => "Sample Module";
        public string Description => "A sample hello-world module demonstrating the plugin platform.";

        public void Initialize(IModuleInitializationContext context)
        {
            context.Navigation.RegisterPage("sample-main", "Sample Module", typeof(SamplePage));

            context.Ribbon.AddButton(
                "sampleButton",
                "Sample Module",
                "Opens the Sample Module page",
                () => { });
        }
    }

    public class SamplePage : Page
    {
        public SamplePage()
        {
            var stack = new System.Windows.Controls.StackPanel
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            stack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Hello from Sample Module!",
                FontSize = 24,
                FontWeight = System.Windows.FontWeights.SemiBold,
                Foreground = System.Windows.Media.Brushes.White,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            });

            stack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "This module was loaded dynamically using the Plugin Platform.",
                FontSize = 14,
                Margin = new System.Windows.Thickness(0, 12, 0, 0),
                Foreground = System.Windows.Media.Brushes.Gray,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            });

            Content = stack;
        }
    }
}
