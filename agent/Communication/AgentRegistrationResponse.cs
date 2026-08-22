namespace Guardian.Agent.Communication;

public sealed record AgentRegistrationResponse(
    bool Succeeded,
    string? AgentId = null,
    string? AgentToken = null,
    int? HeartbeatIntervalSeconds = null,
    string? FailureReason = null)
{
    public static AgentRegistrationResponse Success(
        string agentId,
        string agentToken,
        int heartbeatIntervalSeconds) =>
        new(true, agentId, agentToken, heartbeatIntervalSeconds);

    public static AgentRegistrationResponse Failure(string failureReason) =>
        new(false, FailureReason: failureReason);
}
