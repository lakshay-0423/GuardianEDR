namespace Guardian.Agent.Models;

public sealed record AgentProcessEventRequest(
    string EventType,
    DateTimeOffset Timestamp,
    ProcessEventPayload Payload);

public sealed record ProcessEventPayload(
    int ProcessId,
    string? ProcessName,
    string? ExecutablePath,
    int? ParentProcessId,
    string? ParentProcessName,
    string? CommandLine,
    string? Username);
