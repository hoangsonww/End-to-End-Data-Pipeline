using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface IStreamingService
{
  Task<string> TriggerStreamingAsync(CancellationToken cancellationToken);
  Task<string> GetStreamingStatusAsync(string runId, CancellationToken cancellationToken);
}
