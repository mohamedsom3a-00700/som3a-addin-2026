using System;
using System.Windows;
using System.Windows.Controls;

namespace Som3a_WPF_UI.Pages
{
    public partial class LinksManagerPage : Page
    {
        public LinksManagerPage()
        {
            InitializeComponent();
        }

        public void InitializeWithExcel(object excelApp)
        {
            throw new NotSupportedException("Direct Excel integration is not supported. Use the Relationship Generator page instead.");
        }
    }
}
// TEMP FIX: LinksManagerViewModel reference removed due to pre-existing build error
