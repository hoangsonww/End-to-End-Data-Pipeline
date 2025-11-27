using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using DataPipelineApi.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DataPipelineApi.HealthChecks;

public class MinioHealthCheck : IHealthCheck
{
  private readonly MinioOptions _options;

  public MinioHealthCheck(IOptions<MinioOptions> options) => _options = options.Value;

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      using var client = new AmazonS3Client(_options.AccessKey, _options.SecretKey,
        new AmazonS3Config { ServiceURL = $"http://{_options.Endpoint}", ForcePathStyle = true, Timeout = TimeSpan.FromSeconds(15) });

      var buckets = await client.ListBucketsAsync(cancellationToken);
      var hasBuckets = buckets.Buckets.Any(b => b.BucketName == _options.BucketRaw || b.BucketName == _options.BucketProcessed);
      return hasBuckets ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded("MinIO reachable but buckets missing");
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy("MinIO unreachable", ex);
    }
  }
}
