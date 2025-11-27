using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using DataPipelineApi.Options;

namespace DataPipelineApi.Services
{
    public class AtlasService : IAtlasService
    {
        private readonly HttpClient _http;
        private readonly AtlasOptions _options;

        public AtlasService(HttpClient http, IOptions<AtlasOptions> opt)
        {
            _http = http;
            _options = opt.Value;
            if (_http.BaseAddress is null)
                _http.BaseAddress = new Uri(_options.Endpoint);

            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        }

        public async Task<string> RegisterLineageAsync(string payload, CancellationToken cancellationToken)
        {
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync("/lineage", content, cancellationToken);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
