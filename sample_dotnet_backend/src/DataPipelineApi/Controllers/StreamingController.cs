using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataPipelineApi.Models;
using DataPipelineApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataPipelineApi.Controllers;

[ApiController]
[Route("api/stream")]
public class StreamingController : ControllerBase
{
  private readonly IKafkaService _kafka;
  private readonly IStreamingService _airflow;
  private readonly ILogger<StreamingController> _logger;

  public StreamingController(IKafkaService kafka, IStreamingService airflow, ILogger<StreamingController> logger)
  {
    _kafka = kafka;
    _airflow = airflow;
    _logger = logger;
  }

  [HttpPost("produce")]
  public async Task<IActionResult> Produce([FromBody] StreamingRequest req, CancellationToken cancellationToken)
  {
    var msg = JsonSerializer.Serialize(new { ts = DateTime.UtcNow, partition = req.Partition, payload = req.Payload?.RootElement });
    await _kafka.ProduceAsync(msg, cancellationToken);
    _logger.LogInformation("Produced message to Kafka partition={Partition}", req.Partition);
    return Ok(new { status = "sent" });
  }

  [HttpPost("run")]
  public async Task<ActionResult<StreamingResponse>> Run(CancellationToken cancellationToken)
  {
    var id = await _airflow.TriggerStreamingAsync(cancellationToken);
    _logger.LogInformation("Triggered streaming DAG run {RunId}", id);
    return Ok(new StreamingResponse { RunId = id, Status = "scheduled" });
  }
}
