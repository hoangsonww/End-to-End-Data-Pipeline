using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface ICIService
{
  Task<string> TriggerWorkflowAsync(string workflowFile, string branch, CancellationToken cancellationToken);
}
