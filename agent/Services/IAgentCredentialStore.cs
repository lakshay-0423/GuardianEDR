using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public interface IAgentCredentialStore
{
    Task<AgentCredentialLoadResult> LoadAsync(CancellationToken cancellationToken);

    Task StoreAsync(AgentCredentials credentials, CancellationToken cancellationToken);
}
