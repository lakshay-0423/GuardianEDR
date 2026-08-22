using Guardian.Agent.Models;

namespace Guardian.Agent.Communication;

public interface IAgentRegistrationClient
{
    Task<AgentRegistrationResponse> RegisterAsync(
        AgentRegistrationRequest registrationRequest,
        CancellationToken cancellationToken);
}
