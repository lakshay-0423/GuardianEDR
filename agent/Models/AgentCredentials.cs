namespace Guardian.Agent.Models;

public sealed record AgentCredentials(
    string AgentId,
    string AgentToken,
    int HeartbeatIntervalSeconds,
    DateTimeOffset RegisteredAtUtc);
