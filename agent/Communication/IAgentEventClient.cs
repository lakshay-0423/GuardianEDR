using Guardian.Agent.Models;

namespace Guardian.Agent.Communication;

public interface IAgentEventClient
{
    Task<AgentEventTransmissionResult> SendAsync(
        AgentCredentials credentials,
        ProcessTelemetryEvent processEvent,
        CancellationToken cancellationToken);
}
