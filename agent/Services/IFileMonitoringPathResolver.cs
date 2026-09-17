using Guardian.Agent.Configuration;

namespace Guardian.Agent.Services;

public interface IFileMonitoringPathResolver
{
    IReadOnlyList<string> Resolve(FileMonitoringOptions options);
}
