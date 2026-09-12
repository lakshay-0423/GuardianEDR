using System.Threading.Channels;
using Guardian.Agent.Configuration;
using Guardian.Agent.Models;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class ProcessEventBuffer(
    ILogger<ProcessEventBuffer> logger,
    IOptions<ProcessEventTransmissionOptions> options) : IProcessEventBuffer
{
    private readonly ILogger<ProcessEventBuffer> _logger = logger;
    private readonly Channel<ProcessTelemetryEvent> _events = Channel.CreateBounded<ProcessTelemetryEvent>(
        new BoundedChannelOptions(options.Value.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
    private long _droppedEventCount;

    public void Handle(ProcessTelemetryEvent processEvent)
    {
        if (_events.Writer.TryWrite(processEvent))
        {
            return;
        }

        var droppedEventCount = Interlocked.Increment(ref _droppedEventCount);

        if (droppedEventCount == 1 || droppedEventCount % 100 == 0)
        {
            _logger.LogWarning(
                "Process event buffer is full. Events are being dropped to keep memory bounded. DroppedEventCount: {DroppedEventCount}",
                droppedEventCount);
        }
    }

    public IAsyncEnumerable<ProcessTelemetryEvent> ReadAllAsync(CancellationToken cancellationToken) =>
        _events.Reader.ReadAllAsync(cancellationToken);

    public void Complete() => _events.Writer.TryComplete();
}
