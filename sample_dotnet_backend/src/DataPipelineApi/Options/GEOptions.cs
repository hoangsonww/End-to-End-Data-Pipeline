using System.ComponentModel.DataAnnotations;

namespace DataPipelineApi.Options;

public class GEOptions
{
  [Required, MinLength(1)]
  public string CliPath { get; init; } = string.Empty;

  [Range(5, 1800)]
  public int TimeoutSeconds { get; init; } = 300;
}
