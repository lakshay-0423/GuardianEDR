using Guardian.Agent.Configuration;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class AgentHostedService(
    ILogger<AgentHostedService> logger,
    ISystemInformationCollector systemInformationCollector,
    IOptions<AgentOptions> agentOptions) : IHostedService
{
    private readonly ILogger<AgentHostedService> _logger = logger;
    private readonly ISystemInformationCollector _systemInformationCollector = systemInformationCollector;
    private readonly AgentOptions _agentOptions = agentOptions.Value;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var systemInformation = _systemInformationCollector.Collect();

        _logger.LogInformation(
            "{ServiceName} initialized. Hostname: {Hostname}; WindowsOsName: {WindowsOsName}; WindowsOsVersion: {WindowsOsVersion}; Architecture: {Architecture}; CurrentUsername: {CurrentUsername}; AgentVersion: {AgentVersion}; DeviceIdentifier: {DeviceIdentifier}; LocalIpAddress: {LocalIpAddress}; ShutdownTimeoutSeconds: {ShutdownTimeoutSeconds}",
            _agentOptions.ServiceName,
            systemInformation.Hostname,
            systemInformation.WindowsOsName,
            systemInformation.WindowsOsVersion,
            systemInformation.Architecture,
            systemInformation.CurrentUsername,
            systemInformation.AgentVersion,
            systemInformation.DeviceIdentifier,
            systemInformation.LocalIpAddress,
            _agentOptions.ShutdownTimeoutSeconds);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} is shutting down gracefully.", _agentOptions.ServiceName);
        return Task.CompletedTask;
    }
}
