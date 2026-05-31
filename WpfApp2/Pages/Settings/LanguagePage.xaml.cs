using System.Windows.Controls;
using Som3a_WPF_UI.ViewModels.Settings;

namespace Som3a_WPF_UI.Pages.Settings
{
    public partial class LanguagePage : PageBase
    {
        public LanguagePage()
        {
            InitializeComponent();
            DataContext = new LanguagePageViewModel();
        }
    }
}
