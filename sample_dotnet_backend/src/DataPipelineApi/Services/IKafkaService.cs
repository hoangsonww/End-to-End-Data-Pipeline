using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface IKafkaService
{
  Task ProduceAsync(string message, CancellationToken cancellationToken);
}
