namespace Guardian.Agent.Models;

public sealed record AgentHeartbeatRequest(
    double CpuUsage,
    double MemoryUsage,
    DateTimeOffset Timestamp);
