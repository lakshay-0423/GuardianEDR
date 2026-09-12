using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IProcessEventBuffer : IProcessTelemetrySink
{
    IAsyncEnumerable<ProcessTelemetryEvent> ReadAllAsync(CancellationToken cancellationToken);

    void Complete();
}
