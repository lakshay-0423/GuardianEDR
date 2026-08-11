using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface ISystemInformationCollector
{
    EndpointSystemInformation Collect();
}
