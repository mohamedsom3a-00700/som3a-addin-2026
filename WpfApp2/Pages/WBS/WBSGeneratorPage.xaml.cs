using System.Windows.Controls;
using WpfApp2.Services.WBS;
using WpfApp2.ViewModels.WBS;

namespace WpfApp2.Pages.WBS;

public partial class WBSGeneratorPage : Page
{
    public WBSGeneratorPage(IWBSAIService aiService, IWBSCodeGenerator codeGen, IWBSTreeValidator validator)
    {
        InitializeComponent();
        DataContext = new WBSGeneratorViewModel(aiService, codeGen, validator);
    }
}
