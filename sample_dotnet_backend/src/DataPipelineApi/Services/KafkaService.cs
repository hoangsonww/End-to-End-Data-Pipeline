using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using DataPipelineApi.Options;

namespace DataPipelineApi.Services;
public class KafkaService : IKafkaService, IAsyncDisposable
{
  private readonly IProducer<Null, string> _producer;
  private readonly string _topic;
  private readonly KafkaOptions _options;
  public KafkaService(IOptions<KafkaOptions> opt)
  {
    _options = opt.Value;
    _topic = _options.Topic;
    var cfg = new ProducerConfig
    {
      BootstrapServers = _options.BootstrapServers,
      ClientId = _options.ClientId,
      Acks = Acks.All,
      EnableIdempotence = true,
      MessageSendMaxRetries = 3,
      MessageTimeoutMs = _options.MessageTimeoutMs
    };
    _producer = new ProducerBuilder<Null, string>(cfg).Build();
  }

  public async Task ProduceAsync(string message, CancellationToken cancellationToken)
    => await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message }, cancellationToken);

  public async ValueTask DisposeAsync()
  {
    _producer.Flush(TimeSpan.FromSeconds(_options.ProducerFlushSeconds));
    _producer.Dispose();
    await Task.CompletedTask;
  }
}
