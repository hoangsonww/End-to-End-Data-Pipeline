using System;
using System.Threading;
using System.Threading.Tasks;
using DataPipelineApi.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace DataPipelineApi.HealthChecks;

public class MySqlHealthCheck : IHealthCheck
{
  private readonly DatabaseOptions _options;

  public MySqlHealthCheck(IOptions<DatabaseOptions> options) => _options = options.Value;

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      await using var conn = new MySqlConnection(_options.MySql);
      await conn.OpenAsync(cancellationToken);
      return HealthCheckResult.Healthy();
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy("MySQL connection failed", ex);
    }
  }
}
