using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataPipelineApi.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DataPipelineApi.HealthChecks;

public class AirflowHealthCheck : IHealthCheck
{
  private readonly IHttpClientFactory _clientFactory;
  private readonly AirflowOptions _options;

  public AirflowHealthCheck(IHttpClientFactory clientFactory, IOptions<AirflowOptions> options)
  {
    _clientFactory = clientFactory;
    _options = options.Value;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      var client = _clientFactory.CreateClient("airflow-health");
      var resp = await client.GetAsync("/health", cancellationToken);
      if (!resp.IsSuccessStatusCode)
        return HealthCheckResult.Degraded($"Airflow responded with {resp.StatusCode}");

      var payload = await resp.Content.ReadAsStringAsync(cancellationToken);
      using var doc = JsonDocument.Parse(payload);
      var metadatabase = doc.RootElement.GetProperty("metadatabase").GetProperty("status").GetString();
      var scheduler = doc.RootElement.GetProperty("scheduler").GetProperty("status").GetString();
      var webserver = doc.RootElement.GetProperty("webserver").GetProperty("status").GetString();

      var allOk = string.Equals(metadatabase, "healthy", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(scheduler, "healthy", StringComparison.OrdinalIgnoreCase)
                  && string.Equals(webserver, "healthy", StringComparison.OrdinalIgnoreCase);

      return allOk ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded($"Airflow status: meta={metadatabase}, scheduler={scheduler}, webserver={webserver}");
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy("Airflow health check failed", ex);
    }
  }
}
