using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public sealed class CompositeProcessTelemetrySink(
    StructuredProcessTelemetryLogger telemetryLogger,
    IProcessEventBuffer eventBuffer) : IProcessTelemetrySink
{
    private readonly StructuredProcessTelemetryLogger _telemetryLogger = telemetryLogger;
    private readonly IProcessEventBuffer _eventBuffer = eventBuffer;

    public void Handle(ProcessTelemetryEvent processEvent)
    {
        _telemetryLogger.Handle(processEvent);
        _eventBuffer.Handle(processEvent);
    }
}
