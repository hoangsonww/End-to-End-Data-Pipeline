using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface IBatchService
{
  Task<string> TriggerBatchAsync(CancellationToken cancellationToken);
  Task<string> GetBatchStatusAsync(string runId, CancellationToken cancellationToken);
}
