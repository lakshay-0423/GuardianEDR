using Guardian.Agent.Models;

namespace Guardian.Agent.Communication;

public interface IAgentHeartbeatClient
{
    Task<AgentHeartbeatResult> SendAsync(
        AgentCredentials credentials,
        HeartbeatMetrics metrics,
        CancellationToken cancellationToken);
}
