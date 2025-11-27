using System.Collections.Generic;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataPipelineApi.Services;

public interface IDbService
{
  Task<IReadOnlyList<IDictionary<string, object?>>> ReadMySqlTableAsync(string table, int? limit, CancellationToken cancellationToken);
  Task ExecutePostgresAsync(string sql, CancellationToken cancellationToken);
}
