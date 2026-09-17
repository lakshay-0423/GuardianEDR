using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IFileTelemetrySink
{
    void Handle(FileTelemetryEvent fileEvent);
}
