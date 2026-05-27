using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services
{
    public interface IActivityExportService
    {
        Task ExportAsync(
            IReadOnlyList<GeneratedActivity> activities,
            ActivityExportConfig config,
            CancellationToken ct = default);

        bool SheetExists(string sheetName);
        void RemoveSheet(string sheetName);
    }
}
