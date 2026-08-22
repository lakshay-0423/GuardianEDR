namespace Guardian.Agent.Models;

public sealed record AgentHeartbeatResult(
    bool Succeeded,
    AgentHeartbeatFailure? Failure = null)
{
    public static AgentHeartbeatResult Success() => new(true);

    public static AgentHeartbeatResult Failed(AgentHeartbeatFailure failure) => new(false, failure);
}

public enum AgentHeartbeatFailure
{
    AuthenticationRejected,
    BackendUnavailable,
    RequestTimedOut,
    UnexpectedResponse,
}
