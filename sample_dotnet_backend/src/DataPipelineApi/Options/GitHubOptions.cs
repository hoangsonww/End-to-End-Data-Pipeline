using System.ComponentModel.DataAnnotations;

namespace DataPipelineApi.Options;

public class GitHubOptions
{
  [Required, MinLength(3)]
  public string ActionsApi { get; init; } = string.Empty;

  [Required, MinLength(10)]
  public string Token { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string UserAgent { get; init; } = "data-pipeline-api";
}
