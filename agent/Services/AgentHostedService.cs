using Guardian.Agent.Configuration;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class AgentHostedService(
    ILogger<AgentHostedService> logger,
    ISystemInformationCollector systemInformationCollector,
    IAgentRegistrationService registrationService,
    IOptions<AgentOptions> agentOptions) : IHostedService
{
    private readonly ILogger<AgentHostedService> _logger = logger;
    private readonly ISystemInformationCollector _systemInformationCollector = systemInformationCollector;
    private readonly IAgentRegistrationService _registrationService = registrationService;
    private readonly AgentOptions _agentOptions = agentOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
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

        var registration = await _registrationService.RegisterAsync(systemInformation, cancellationToken);

        if (!registration.Succeeded)
        {
            _logger.LogWarning(
                "Endpoint registration was not completed. Reason: {Reason}",
                registration.FailureReason);
            return;
        }

        _logger.LogInformation(
            "Endpoint registration completed. AgentId: {AgentId}; HeartbeatIntervalSeconds: {HeartbeatIntervalSeconds}",
            registration.AgentId,
            registration.HeartbeatIntervalSeconds);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} is shutting down gracefully.", _agentOptions.ServiceName);
        return Task.CompletedTask;
    }
}
