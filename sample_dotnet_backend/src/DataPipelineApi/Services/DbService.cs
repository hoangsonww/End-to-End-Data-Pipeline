using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using Npgsql;
using Microsoft.Extensions.Options;
using DataPipelineApi.Options;

namespace DataPipelineApi.Services;
public class DbService : IDbService
{
  private static readonly Regex TableNameRegex = new("^[A-Za-z0-9_]+$", RegexOptions.Compiled);

  private readonly string _myCs, _pgCs;
  private readonly int _commandTimeout;
  public DbService(IOptions<DatabaseOptions> opt)
  {
    var cfg = opt.Value;
    _myCs = cfg.MySql;
    _pgCs = cfg.Postgres;
    _commandTimeout = cfg.CommandTimeoutSeconds;
  }

  public async Task<IReadOnlyList<IDictionary<string, object?>>> ReadMySqlTableAsync(
    string table,
    int? limit,
    CancellationToken cancellationToken)
  {
    if (!TableNameRegex.IsMatch(table))
      throw new ArgumentException("Only alphanumeric and underscore table names are allowed", nameof(table));

    var sql = limit.HasValue
      ? $"SELECT * FROM `{table}` LIMIT @limit"
      : $"SELECT * FROM `{table}`";

    await using var conn = new MySqlConnection(_myCs);
    var cmd = new CommandDefinition(sql, new { limit }, commandTimeout: _commandTimeout, cancellationToken: cancellationToken);
    var rows = await conn.QueryAsync(cmd);
    return rows.Select(r => (IDictionary<string, object?>)r).ToList();
  }

  public async Task ExecutePostgresAsync(string sql, CancellationToken cancellationToken)
  {
    await using var conn = new NpgsqlConnection(_pgCs);
    var cmd = new CommandDefinition(sql, commandTimeout: _commandTimeout, cancellationToken: cancellationToken);
    await conn.ExecuteAsync(cmd);
  }
}
