using Guardian.Agent.Configuration;
using Guardian.Agent.Models;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class AgentHostedService(
    ILogger<AgentHostedService> logger,
    ISystemInformationCollector systemInformationCollector,
    IAgentCredentialStore credentialStore,
    IAgentRegistrationService registrationService,
    IOptions<AgentOptions> agentOptions) : IHostedService
{
    private readonly ILogger<AgentHostedService> _logger = logger;
    private readonly ISystemInformationCollector _systemInformationCollector = systemInformationCollector;
    private readonly IAgentCredentialStore _credentialStore = credentialStore;
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

        var storedCredentials = await _credentialStore.LoadAsync(cancellationToken);

        if (storedCredentials.Status == AgentCredentialLoadStatus.Available)
        {
            _logger.LogInformation(
                "Recovered protected agent credentials. AgentId: {AgentId}; HeartbeatIntervalSeconds: {HeartbeatIntervalSeconds}",
                storedCredentials.Credentials!.AgentId,
                storedCredentials.Credentials.HeartbeatIntervalSeconds);
            return;
        }

        if (storedCredentials.Status == AgentCredentialLoadStatus.Unavailable)
        {
            _logger.LogWarning(
                "Stored agent credentials cannot be accessed. Registration will not be attempted to avoid replacing unavailable credentials.");
            return;
        }

        if (storedCredentials.Status == AgentCredentialLoadStatus.Corrupted)
        {
            _logger.LogWarning(
                "Stored agent credentials are invalid or cannot be decrypted. A new registration will be requested.");
        }

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
