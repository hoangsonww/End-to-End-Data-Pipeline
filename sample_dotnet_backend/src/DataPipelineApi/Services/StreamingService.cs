using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using DataPipelineApi.Options;

namespace DataPipelineApi.Services;
public class StreamingService : IStreamingService
{
  private readonly HttpClient _http;
  private readonly AirflowOptions _options;
  public StreamingService(HttpClient http, IOptions<AirflowOptions> opt)
  {
    _http = http;
    _options = opt.Value;
    if (_http.BaseAddress is null)
      _http.BaseAddress = new Uri(_options.BaseUrl);
    var tok = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", tok);
  }
  public async Task<string> TriggerStreamingAsync(CancellationToken cancellationToken)
  {
    var id = $"stream_{DateTime.UtcNow:yyyyMMddHHmmss}";
    var resp = await _http.PostAsJsonAsync($"/dags/{_options.StreamingDagId}/dagRuns", new { dag_run_id = id }, cancellationToken);
    resp.EnsureSuccessStatusCode();
    return id;
  }
  public async Task<string> GetStreamingStatusAsync(string runId, CancellationToken cancellationToken)
  {
    var r = await _http.GetAsync($"/dags/{_options.StreamingDagId}/dagRuns/{runId}", cancellationToken);
    r.EnsureSuccessStatusCode();
    return await r.Content.ReadAsStringAsync(cancellationToken);
  }
}
