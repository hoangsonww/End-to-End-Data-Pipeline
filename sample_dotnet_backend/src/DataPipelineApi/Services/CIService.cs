using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using DataPipelineApi.Options;

namespace DataPipelineApi.Services;
public class CIService : ICIService
{
  private readonly HttpClient _http;
  private readonly GitHubOptions _options;
  public CIService(HttpClient http, IOptions<GitHubOptions> opt)
  {
    _http = http;
    _options = opt.Value;
    if (_http.BaseAddress is null)
    {
      var baseUri = _options.ActionsApi.EndsWith("/") ? _options.ActionsApi : $"{_options.ActionsApi}/";
      _http.BaseAddress = new Uri(baseUri);
    }

    _http.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", _options.Token);
    _http.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
  }
  public async Task<string> TriggerWorkflowAsync(string wf, string branch, CancellationToken cancellationToken)
  {
    var payload = new { @ref = branch };
    var r = await _http.PostAsJsonAsync($"{wf}/dispatches", payload, cancellationToken);
    r.EnsureSuccessStatusCode();
    return await r.Content.ReadAsStringAsync(cancellationToken);
  }
}
