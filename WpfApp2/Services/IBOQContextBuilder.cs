using System.Threading;
using System.Threading.Tasks;
using Som3a_WPF_UI.Models;

namespace Som3a_WPF_UI.Services
{
    public interface IBOQContextBuilder
    {
        Task<BOQContext> BuildContextAsync(CancellationToken ct = default);
        bool TryIdentifyBoqSheet(out string? sheetName);
    }
}
