using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public sealed class StructuredFileTelemetryLogger(
    ILogger<StructuredFileTelemetryLogger> logger) : IFileTelemetrySink
{
    private readonly ILogger<StructuredFileTelemetryLogger> _logger = logger;

    public void Handle(FileTelemetryEvent fileEvent)
    {
        _logger.LogInformation(
            "File telemetry collected. EventType: {EventType}; FullPath: {FullPath}; FileName: {FileName}; Extension: {Extension}; Timestamp: {Timestamp}; FileSizeBytes: {FileSizeBytes}; PreviousPath: {PreviousPath}",
            fileEvent.EventType,
            fileEvent.FullPath,
            fileEvent.FileName,
            fileEvent.Extension,
            fileEvent.Timestamp,
            fileEvent.FileSizeBytes,
            fileEvent.PreviousPath);
    }
}
