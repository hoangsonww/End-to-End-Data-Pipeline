using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DataPipelineApi.Services;

namespace DataPipelineApi.Controllers;
[ApiController]
[Route("api/ci")]
public class CIController : ControllerBase
{
  private readonly ICIService _ci;
  public CIController(ICIService ci) => _ci = ci;

  [HttpPost("trigger")]
  public async Task<IActionResult> Trigger([FromQuery] string wf, [FromQuery] string branch, CancellationToken cancellationToken)
  {
    var res = await _ci.TriggerWorkflowAsync(wf, branch, cancellationToken);
    return Ok(new { result = res });
  }
}
