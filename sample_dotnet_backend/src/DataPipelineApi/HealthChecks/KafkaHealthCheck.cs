using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using DataPipelineApi.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DataPipelineApi.HealthChecks;

public class KafkaHealthCheck : IHealthCheck
{
  private readonly KafkaOptions _options;

  public KafkaHealthCheck(IOptions<KafkaOptions> options) => _options = options.Value;

  public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      var config = new AdminClientConfig
      {
        BootstrapServers = _options.BootstrapServers,
        ClientId = $"{_options.ClientId}-health"
      };
      using var admin = new AdminClientBuilder(config).Build();
      var metadata = admin.GetMetadata(_options.Topic, TimeSpan.FromSeconds(5));
      var topic = metadata.Topics.FirstOrDefault(t => t.Topic == _options.Topic);
      if (topic is null || topic.Error.Code != ErrorCode.NoError)
        return Task.FromResult(HealthCheckResult.Degraded($"Kafka topic '{_options.Topic}' not available: {topic?.Error}"));

      return Task.FromResult(HealthCheckResult.Healthy());
    }
    catch (Exception ex)
    {
      return Task.FromResult(HealthCheckResult.Unhealthy("Kafka unavailable", ex));
    }
  }
}
