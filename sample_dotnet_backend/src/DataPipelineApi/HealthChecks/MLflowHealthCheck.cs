using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataPipelineApi.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DataPipelineApi.HealthChecks;

public class MLflowHealthCheck : IHealthCheck
{
  private readonly IHttpClientFactory _clientFactory;
  private readonly MLflowOptions _options;

  public MLflowHealthCheck(IHttpClientFactory clientFactory, IOptions<MLflowOptions> options)
  {
    _clientFactory = clientFactory;
    _options = options.Value;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      var client = _clientFactory.CreateClient("mlflow-health");
      var resp = await client.GetAsync("/api/2.0/mlflow/experiments/list?max_results=1", cancellationToken);
      if (!resp.IsSuccessStatusCode)
        return HealthCheckResult.Degraded($"MLflow responded with {resp.StatusCode}");

      return HealthCheckResult.Healthy();
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy("MLflow unreachable", ex);
    }
  }
}
