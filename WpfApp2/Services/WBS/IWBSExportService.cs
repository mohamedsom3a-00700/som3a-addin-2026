using Som3a.Domain.WBS;

namespace WpfApp2.Services.WBS;

public interface IWBSExportService
{
    Task ExportToExcelAsync(WBSNode root, string filePath);
    Task ExportToJsonAsync(WBSNode root, string filePath);
    Task ExportToXmlAsync(WBSNode root, string filePath);
}
