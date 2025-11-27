using System;
using System.Threading;
using System.Threading.Tasks;
using DataPipelineApi.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DataPipelineApi.HealthChecks;

public class PostgresHealthCheck : IHealthCheck
{
  private readonly DatabaseOptions _options;

  public PostgresHealthCheck(IOptions<DatabaseOptions> options) => _options = options.Value;

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      await using var conn = new NpgsqlConnection(_options.Postgres);
      await conn.OpenAsync(cancellationToken);
      return HealthCheckResult.Healthy();
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy("PostgreSQL connection failed", ex);
    }
  }
}
