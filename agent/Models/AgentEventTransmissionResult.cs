namespace Guardian.Agent.Models;

public enum AgentEventTransmissionFailure
{
    AuthenticationRejected,
    BackendUnavailable,
    InvalidPayload,
    RequestTimedOut,
    UnexpectedResponse,
}

public sealed record AgentEventTransmissionResult(
    bool Succeeded,
    AgentEventTransmissionFailure? Failure)
{
    public static AgentEventTransmissionResult Success() => new(true, null);

    public static AgentEventTransmissionResult Failed(AgentEventTransmissionFailure failure) => new(false, failure);
}
