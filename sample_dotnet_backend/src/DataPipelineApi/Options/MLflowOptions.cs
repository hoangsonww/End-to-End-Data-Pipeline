using System.ComponentModel.DataAnnotations;

namespace DataPipelineApi.Options;

public class MLflowOptions
{
  [Required, MinLength(3)]
  public string TrackingUri { get; init; } = string.Empty;

  [Range(5, 300)]
  public int RequestTimeoutSeconds { get; init; } = 30;
}
