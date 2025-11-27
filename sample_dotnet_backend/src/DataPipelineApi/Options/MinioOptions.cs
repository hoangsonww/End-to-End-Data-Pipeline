using System.ComponentModel.DataAnnotations;

namespace DataPipelineApi.Options;

public class MinioOptions
{
  [Required, MinLength(3)]
  public string Endpoint { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string AccessKey { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string SecretKey { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string BucketRaw { get; init; } = string.Empty;

  [Required, MinLength(1)]
  public string BucketProcessed { get; init; } = string.Empty;

  [Range(1, 4)]
  public int MaxUploadRetries { get; init; } = 2;
}
