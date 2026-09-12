using Guardian.Agent.Configuration;
using Guardian.Agent.Models;
using Microsoft.Extensions.Options;
using System.Management;

namespace Guardian.Agent.Services;

public sealed class ProcessMonitoringHostedService(
    ILogger<ProcessMonitoringHostedService> logger,
    IProcessMonitor processMonitor,
    IProcessTelemetrySink processTelemetrySink,
    IOptions<ProcessMonitoringOptions> processMonitoringOptions) : IHostedService
{
    private readonly ILogger<ProcessMonitoringHostedService> _logger = logger;
    private readonly IProcessMonitor _processMonitor = processMonitor;
    private readonly IProcessTelemetrySink _processTelemetrySink = processTelemetrySink;
    private readonly ProcessMonitoringOptions _processMonitoringOptions = processMonitoringOptions.Value;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_processMonitoringOptions.Enabled)
        {
            _logger.LogInformation("Process monitoring is disabled by configuration.");
            return Task.CompletedTask;
        }

        _processMonitor.ProcessEventReceived += HandleProcessEvent;

        try
        {
            _processMonitor.Start();
            _logger.LogInformation("Windows process monitoring started.");
        }
        catch (ManagementException exception)
        {
            _processMonitor.ProcessEventReceived -= HandleProcessEvent;
            _logger.LogError(
                "Windows process monitoring could not start. ErrorType: {ErrorType}",
                exception.GetType().Name);
        }
        catch (UnauthorizedAccessException exception)
        {
            _processMonitor.ProcessEventReceived -= HandleProcessEvent;
            _logger.LogError(
                "Windows process monitoring could not start. ErrorType: {ErrorType}",
                exception.GetType().Name);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _processMonitor.ProcessEventReceived -= HandleProcessEvent;
        _processMonitor.Stop();
        _logger.LogInformation("Windows process monitoring stopped.");
        return Task.CompletedTask;
    }

    private void HandleProcessEvent(object? sender, ProcessTelemetryEvent processEvent) =>
        _processTelemetrySink.Handle(processEvent);
}
