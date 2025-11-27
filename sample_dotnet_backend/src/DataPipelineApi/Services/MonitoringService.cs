using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DataPipelineApi.Services;
public class MonitoringService : IMonitoringService
{
  private readonly HealthCheckService _healthChecks;

  public MonitoringService(HealthCheckService healthChecks) => _healthChecks = healthChecks;

  public async Task<IDictionary<string, string>> GetHealthAsync(CancellationToken cancellationToken)
  {
    var report = await _healthChecks.CheckHealthAsync(cancellationToken);
    var status = report.Entries.ToDictionary(x => x.Key, x => x.Value.Status.ToString());
    status["overall"] = report.Status.ToString();
    return status;
  }
}
