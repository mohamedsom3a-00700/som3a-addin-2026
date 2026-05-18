namespace Som3a_WPF_UI
{
    public partial class XerEditorWindow : Som3a_WPF_UI.Controls.ModernWindow
    {
        public XerEditorWindow()
        {
            InitializeComponent();
            DataContext = new XerEditorVM();
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