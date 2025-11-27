using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface IAtlasService
{
  Task<string> RegisterLineageAsync(string payload, CancellationToken cancellationToken);
}
