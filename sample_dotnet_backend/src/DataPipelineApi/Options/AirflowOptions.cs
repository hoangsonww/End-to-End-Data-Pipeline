using System.ComponentModel.DataAnnotations;

namespace DataPipelineApi.Options;

public class AirflowOptions
{
  [Required, MinLength(1)]
  public string BaseUrl { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string Username { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string Password { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string BatchDagId { get; init; } = "batch_ingestion_dag";

  [Required, MinLength(1)]
  public string StreamingDagId { get; init; } = "streaming_monitoring_dag";

  [Range(5, 300)]
  public int RequestTimeoutSeconds { get; init; } = 30;
}
