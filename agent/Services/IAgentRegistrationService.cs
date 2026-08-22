using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IAgentRegistrationService
{
    Task<AgentRegistrationResult> RegisterAsync(
        EndpointSystemInformation systemInformation,
        CancellationToken cancellationToken);
}
