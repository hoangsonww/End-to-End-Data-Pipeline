using System.ComponentModel.DataAnnotations;

namespace DataPipelineApi.Options;

public class DatabaseOptions
{
  [Required, MinLength(1)]
  public string MySql { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string Postgres { get; init; } = string.Empty;

  [Range(5, 300)]
  public int CommandTimeoutSeconds { get; init; } = 30;
}
