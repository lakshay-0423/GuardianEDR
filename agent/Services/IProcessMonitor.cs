using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IProcessMonitor : IDisposable
{
    event EventHandler<ProcessTelemetryEvent>? ProcessEventReceived;

    void Start();

    void Stop();
}
