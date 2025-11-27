using System.Threading;
using System.Threading.Tasks;
using DataPipelineApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataPipelineApi.Controllers;
[ApiController]
[Route("api/monitor")]
public class MonitoringController : ControllerBase
{
  private readonly IMonitoringService _mon;
  public MonitoringController(IMonitoringService mon) => _mon = mon;

  [HttpGet("health")]
  public async Task<IActionResult> Health(CancellationToken cancellationToken)
    => Ok(await _mon.GetHealthAsync(cancellationToken));
}
