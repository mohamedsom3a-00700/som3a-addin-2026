using Som3a_WPF_UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Pages
{
    public partial class XerEditorPage : Page
    {
        public XerEditorPage()
        {
            InitializeComponent();
            DataContext = new XerEditorViewModel(App.Container);
        }
    }
}
