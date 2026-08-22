using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IAgentCredentialStore
{
    Task StoreAsync(AgentCredentials credentials, CancellationToken cancellationToken);
}
