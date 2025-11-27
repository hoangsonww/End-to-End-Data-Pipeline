using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using DataPipelineApi.Options;

namespace DataPipelineApi.Services;
public class GEValidationService : IGEValidationService
{
  private readonly string _cli;
  private readonly int _timeoutSeconds;

  public GEValidationService(IOptions<GEOptions> opt)
  {
    var cfg = opt.Value;
    _cli = cfg.CliPath;
    _timeoutSeconds = cfg.TimeoutSeconds;
  }

  public async Task<string> ValidateAsync(string suite, CancellationToken cancellationToken)
  {
    if (!File.Exists(_cli))
      throw new FileNotFoundException($"Great Expectations CLI not found at {_cli}");

    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

    var psi = new ProcessStartInfo(_cli, $"checkpoint run {suite}")
    {
      RedirectStandardError = true,
      RedirectStandardOutput = true,
      UseShellExecute = false
    };

    using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start Great Expectations CLI");
    var stdoutTask = process.StandardOutput.ReadToEndAsync();
    var stderrTask = process.StandardError.ReadToEndAsync();

    await Task.WhenAll(process.WaitForExitAsync(cts.Token), stdoutTask, stderrTask);
    if (process.ExitCode != 0)
      throw new InvalidOperationException($"Great Expectations validation failed: {await stderrTask}");

    return await stdoutTask;
  }
}
