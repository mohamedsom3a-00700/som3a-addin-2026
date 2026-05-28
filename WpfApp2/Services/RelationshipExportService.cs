using Som3a_WPF_UI.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public class RelationshipExportService : IRelationshipExportService
    {
        public Task ExportToExcelAsync(
            IReadOnlyList<Relationship> relationships,
            CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
