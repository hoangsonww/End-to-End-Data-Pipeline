using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface IMLflowService
{
  Task<string> CreateRunAsync(string experimentId, string runName, CancellationToken cancellationToken);
}
