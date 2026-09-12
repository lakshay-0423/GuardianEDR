using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public sealed class StructuredProcessTelemetryLogger(
    ILogger<StructuredProcessTelemetryLogger> logger) : IProcessTelemetrySink
{
    private readonly ILogger<StructuredProcessTelemetryLogger> _logger = logger;

    public void Handle(ProcessTelemetryEvent processEvent)
    {
        _logger.LogInformation(
            "Process telemetry collected. EventType: {EventType}; ProcessId: {ProcessId}; ProcessName: {ProcessName}; ParentProcessId: {ParentProcessId}; ParentProcessName: {ParentProcessName}; HasExecutablePath: {HasExecutablePath}; HasCommandLine: {HasCommandLine}; HasUsername: {HasUsername}",
            processEvent.EventType,
            processEvent.ProcessId,
            processEvent.ProcessName,
            processEvent.ParentProcessId,
            processEvent.ParentProcessName,
            !string.IsNullOrWhiteSpace(processEvent.ExecutablePath),
            !string.IsNullOrWhiteSpace(processEvent.CommandLine),
            !string.IsNullOrWhiteSpace(processEvent.Username));
    }
}
