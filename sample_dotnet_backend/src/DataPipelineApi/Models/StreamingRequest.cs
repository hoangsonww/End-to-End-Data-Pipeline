using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DataPipelineApi.Models;

public class StreamingRequest
{
  [Range(0, int.MaxValue)]
  public int Partition { get; set; } = 0;

  public JsonDocument? Payload { get; set; }
}

public class StreamingResponse
{
  public string RunId { get; set; } = string.Empty;
  public string? Status { get; set; }
}
