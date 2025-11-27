using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using DataPipelineApi.Options;

namespace DataPipelineApi.Services;
public class MinioService : IStorageService
{
  private readonly AmazonS3Client _s3;
  private readonly string _bRaw, _bProc;
  private readonly MinioOptions _options;
  private readonly SemaphoreSlim _bucketGate = new(1, 1);
  private bool _bucketsEnsured;
  public MinioService(IOptions<MinioOptions> opt)
  {
    _options = opt.Value;
    _s3 = new AmazonS3Client(_options.AccessKey, _options.SecretKey,
      new AmazonS3Config { ServiceURL = $"http://{_options.Endpoint}", ForcePathStyle = true, Timeout = TimeSpan.FromSeconds(30) });
    _bRaw = _options.BucketRaw;
    _bProc = _options.BucketProcessed;
  }

  public async Task UploadRawAsync(string key, Stream data, CancellationToken cancellationToken)
  {
    await EnsureBucketsAsync(cancellationToken);
    await PutWithRetryAsync(new PutObjectRequest { BucketName = _bRaw, Key = key, InputStream = data }, cancellationToken);
  }

  public async Task<Stream> DownloadRawAsync(string key, CancellationToken cancellationToken)
  {
    await EnsureBucketsAsync(cancellationToken);
    var r = await _s3.GetObjectAsync(_bRaw, key, cancellationToken);
    var ms = new MemoryStream();
    await r.ResponseStream.CopyToAsync(ms, cancellationToken);
    ms.Position = 0;
    return ms;
  }

  public async Task UploadProcessedAsync(string key, Stream data, CancellationToken cancellationToken)
  {
    await EnsureBucketsAsync(cancellationToken);
    await PutWithRetryAsync(new PutObjectRequest { BucketName = _bProc, Key = key, InputStream = data }, cancellationToken);
  }

  private async Task EnsureBucketsAsync(CancellationToken cancellationToken)
  {
    if (_bucketsEnsured) return;

    await _bucketGate.WaitAsync(cancellationToken);
    try
    {
      if (_bucketsEnsured) return;
      await EnsureBucketAsync(_bRaw, cancellationToken);
      await EnsureBucketAsync(_bProc, cancellationToken);
      _bucketsEnsured = true;
    }
    finally
    {
      _bucketGate.Release();
    }
  }

  private async Task EnsureBucketAsync(string bucketName, CancellationToken cancellationToken)
  {
    var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3, bucketName);
    if (!exists)
      await _s3.PutBucketAsync(new PutBucketRequest { BucketName = bucketName }, cancellationToken);
  }

  private async Task PutWithRetryAsync(PutObjectRequest request, CancellationToken cancellationToken)
  {
    var attempts = 0;
    while (true)
    {
      attempts++;
      try
      {
        await _s3.PutObjectAsync(request, cancellationToken);
        return;
      }
      catch when (attempts <= _options.MaxUploadRetries)
      {
        await Task.Delay(TimeSpan.FromMilliseconds(200 * attempts), cancellationToken);
      }
    }
  }
}
