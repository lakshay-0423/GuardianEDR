namespace Guardian.Agent.Models;

public sealed record FileTelemetryEvent(
    FileEventType EventType,
    string FullPath,
    string FileName,
    string Extension,
    DateTimeOffset Timestamp,
    long? FileSizeBytes,
    string? PreviousPath);
