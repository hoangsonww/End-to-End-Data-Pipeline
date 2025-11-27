using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface IStorageService
{
  Task UploadRawAsync(string key, Stream data, CancellationToken cancellationToken);
  Task<Stream> DownloadRawAsync(string key, CancellationToken cancellationToken);
  Task UploadProcessedAsync(string key, Stream data, CancellationToken cancellationToken);
}
