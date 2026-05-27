using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services.WBS;

public interface IWBSExportService
{
    Task ExportToExcelAsync(WBSNode root, string filePath);
    Task ExportToJsonAsync(WBSNode root, string filePath);
    Task ExportToXmlAsync(WBSNode root, string filePath);
}
