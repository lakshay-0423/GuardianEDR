using Guardian.Agent.Communication;
using Guardian.Agent.Configuration;
using Guardian.Agent.Models;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class HeartbeatHostedService(
    ILogger<HeartbeatHostedService> logger,
    IAgentCredentialStore credentialStore,
    IAgentHeartbeatClient heartbeatClient,
    IHeartbeatMetricsCollector metricsCollector,
    IOptions<HeartbeatOptions> heartbeatOptions) : BackgroundService
{
    private readonly ILogger<HeartbeatHostedService> _logger = logger;
    private readonly IAgentCredentialStore _credentialStore = credentialStore;
    private readonly IAgentHeartbeatClient _heartbeatClient = heartbeatClient;
    private readonly IHeartbeatMetricsCollector _metricsCollector = metricsCollector;
    private readonly HeartbeatOptions _heartbeatOptions = heartbeatOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var credentialLoadResult = await _credentialStore.LoadAsync(stoppingToken);

        if (credentialLoadResult.Status != AgentCredentialLoadStatus.Available)
        {
            _logger.LogWarning(
                "Heartbeat service is waiting for valid agent credentials. CredentialStatus: {CredentialStatus}",
                credentialLoadResult.Status);
            return;
        }

        var credentials = credentialLoadResult.Credentials!;
        var intervalSeconds = credentials.HeartbeatIntervalSeconds > 0
            ? credentials.HeartbeatIntervalSeconds
            : _heartbeatOptions.FallbackIntervalSeconds;
        var interval = TimeSpan.FromSeconds(intervalSeconds);

        _logger.LogInformation(
            "Heartbeat service started. AgentId: {AgentId}; IntervalSeconds: {IntervalSeconds}",
            credentials.AgentId,
            intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var heartbeat = await _heartbeatClient.SendAsync(
                credentials,
                _metricsCollector.Collect(),
                stoppingToken);

            if (heartbeat.Succeeded)
            {
                _logger.LogInformation("Heartbeat sent successfully. AgentId: {AgentId}", credentials.AgentId);
            }
            else
            {
                _logger.LogWarning(
                    "Heartbeat was not delivered. AgentId: {AgentId}; Reason: {Reason}",
                    credentials.AgentId,
                    heartbeat.Failure);
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
