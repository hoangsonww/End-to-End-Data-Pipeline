using System.ComponentModel.DataAnnotations;

namespace DataPipelineApi.Options;

public class AtlasOptions
{
  [Required, MinLength(3)]
  public string Endpoint { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string Username { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string Password { get; init; } = string.Empty;
}
