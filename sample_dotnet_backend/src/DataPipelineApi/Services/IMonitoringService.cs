using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface IMonitoringService
{
  Task<IDictionary<string, string>> GetHealthAsync(CancellationToken cancellationToken);
}
