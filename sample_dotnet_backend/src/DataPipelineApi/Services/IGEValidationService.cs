using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface IGEValidationService
{
  Task<string> ValidateAsync(string suite, CancellationToken cancellationToken);
}
