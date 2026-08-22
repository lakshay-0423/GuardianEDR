namespace Guardian.Agent.Models;

public sealed record AgentRegistrationResult(
    bool Succeeded,
    string? AgentId = null,
    int? HeartbeatIntervalSeconds = null,
    string? FailureReason = null)
{
    public static AgentRegistrationResult Success(string agentId, int heartbeatIntervalSeconds) =>
        new(true, agentId, heartbeatIntervalSeconds);

    public static AgentRegistrationResult Failure(string failureReason) =>
        new(false, FailureReason: failureReason);
}
