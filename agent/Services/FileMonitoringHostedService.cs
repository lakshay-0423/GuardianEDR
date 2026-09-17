using Guardian.Agent.Configuration;
using Guardian.Agent.Models;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class FileMonitoringHostedService(
    ILogger<FileMonitoringHostedService> logger,
    IFileMonitor fileMonitor,
    IFileTelemetrySink fileTelemetrySink,
    IOptions<FileMonitoringOptions> fileMonitoringOptions) : IHostedService
{
    private readonly ILogger<FileMonitoringHostedService> _logger = logger;
    private readonly IFileMonitor _fileMonitor = fileMonitor;
    private readonly IFileTelemetrySink _fileTelemetrySink = fileTelemetrySink;
    private readonly FileMonitoringOptions _fileMonitoringOptions = fileMonitoringOptions.Value;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_fileMonitoringOptions.Enabled)
        {
            _logger.LogInformation("File monitoring is disabled by configuration.");
            return Task.CompletedTask;
        }

        _fileMonitor.FileEventReceived += HandleFileEvent;

        try
        {
            _fileMonitor.Start();
        }
        catch (Exception exception) when (
            exception is IOException
            or UnauthorizedAccessException)
        {
            _fileMonitor.FileEventReceived -= HandleFileEvent;
            _logger.LogError(
                "Windows file monitoring could not start. ErrorType: {ErrorType}",
                exception.GetType().Name);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _fileMonitor.FileEventReceived -= HandleFileEvent;
        _fileMonitor.Stop();
        _logger.LogInformation("Windows file monitoring stopped.");
        return Task.CompletedTask;
    }

    private void HandleFileEvent(object? sender, FileTelemetryEvent fileEvent) =>
        _fileTelemetrySink.Handle(fileEvent);
}
