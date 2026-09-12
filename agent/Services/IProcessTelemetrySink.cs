using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IProcessTelemetrySink
{
    void Handle(ProcessTelemetryEvent processEvent);
}
