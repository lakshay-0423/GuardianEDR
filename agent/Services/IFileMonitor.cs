using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IFileMonitor : IDisposable
{
    event EventHandler<FileTelemetryEvent>? FileEventReceived;

    void Start();

    void Stop();
}
