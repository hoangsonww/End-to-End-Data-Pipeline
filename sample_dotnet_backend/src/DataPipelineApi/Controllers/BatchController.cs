using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataPipelineApi.Models;
using DataPipelineApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataPipelineApi.Controllers;

[ApiController]
[Route("api/batch")]
public class BatchController : ControllerBase
{
  private readonly IDbService _db;
  private readonly IStorageService _storage;
  private readonly IGEValidationService _ge;
  private readonly IBatchService _airflow;
  private readonly ILogger<BatchController> _logger;

  public BatchController(IDbService db, IStorageService storage, IGEValidationService ge, IBatchService airflow, ILogger<BatchController> logger)
  {
    _db = db;
    _storage = storage;
    _ge = ge;
    _airflow = airflow;
    _logger = logger;
  }

  [HttpPost("ingest")]
  public async Task<ActionResult<BatchResponse>> Ingest([FromBody] BatchRequest req, CancellationToken cancellationToken)
  {
    var rows = await _db.ReadMySqlTableAsync(req.SourceTable, req.Limit, cancellationToken);
    await using var ms = new MemoryStream();
    await JsonSerializer.SerializeAsync(ms, rows, cancellationToken: cancellationToken);
    ms.Position = 0;

    var prefix = string.IsNullOrWhiteSpace(req.DestinationPrefix) ? req.SourceTable : req.DestinationPrefix.Trim('/');
    if (prefix.Contains("..", StringComparison.Ordinal))
      return ValidationProblem("destinationPrefix cannot contain path traversal sequences");
    var objectKey = $"{prefix}/{DateTime.UtcNow:yyyyMMddHHmmss}.json";
    await _storage.UploadRawAsync(objectKey, ms, cancellationToken);

    string? report = null;
    if (req.RunGreatExpectations)
      report = await _ge.ValidateAsync("great_expectations/expectations", cancellationToken);

    string? run = null;
    if (req.TriggerAirflow)
      run = await _airflow.TriggerBatchAsync(cancellationToken);

    _logger.LogInformation("Batch ingest completed for {Table} rows={RowCount} objectKey={ObjectKey} run={RunId}", req.SourceTable, rows.Count, objectKey, run);
    return Ok(new BatchResponse { RunId = run, GEReport = report, ObjectKey = objectKey });
  }
}
