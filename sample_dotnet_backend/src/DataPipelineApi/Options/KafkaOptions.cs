using System.ComponentModel.DataAnnotations;

namespace DataPipelineApi.Options;

public class KafkaOptions
{
  [Required, MinLength(3)]
  public string BootstrapServers { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string Topic { get; init; } = string.Empty;

  [MinLength(1)]
  public string ClientId { get; init; } = "data-pipeline-api";

  [Range(500, 30000)]
  public int MessageTimeoutMs { get; init; } = 5000;

  [Range(1, 10)]
  public int ProducerFlushSeconds { get; init; } = 2;
}
