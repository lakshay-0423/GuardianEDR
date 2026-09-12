namespace Guardian.Agent.Models;

public sealed record ProcessTelemetryEvent(
    ProcessEventType EventType,
    int ProcessId,
    string? ProcessName,
    string? ExecutablePath,
    int? ParentProcessId,
    string? ParentProcessName,
    string? CommandLine,
    string? Username,
    DateTimeOffset Timestamp);
