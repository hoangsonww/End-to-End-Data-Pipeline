using System.ComponentModel.DataAnnotations;

namespace DataPipelineApi.Models;

public class BatchRequest
{
  [Required, RegularExpression("^[A-Za-z0-9_]+$")]
  public string SourceTable { get; set; } = string.Empty;

  [StringLength(200)]
  public string? DestinationPrefix { get; set; }

  [Range(1, 100000)]
  public int? Limit { get; set; }

  public bool TriggerAirflow { get; set; } = true;

  public bool RunGreatExpectations { get; set; } = true;
}

public class BatchResponse
{
  public string ObjectKey { get; set; } = string.Empty;
  public string? RunId { get; set; }
  public string? GEReport { get; set; }
}
