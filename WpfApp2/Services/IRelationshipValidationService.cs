using Som3a_WPF_UI.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Som3a_WPF_UI.Services
{
    public interface IRelationshipValidationService
    {
        Task<ValidationReport> ValidateNetworkAsync(
            RelationshipNetwork network,
            CancellationToken ct = default);
    }
}
