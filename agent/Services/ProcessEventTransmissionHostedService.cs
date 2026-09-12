using Guardian.Agent.Communication;
using Guardian.Agent.Configuration;
using Guardian.Agent.Models;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class ProcessEventTransmissionHostedService(
    ILogger<ProcessEventTransmissionHostedService> logger,
    IProcessEventBuffer eventBuffer,
    IAgentCredentialStore credentialStore,
    IAgentEventClient eventClient,
    IOptions<ProcessEventTransmissionOptions> options) : BackgroundService
{
    private readonly ILogger<ProcessEventTransmissionHostedService> _logger = logger;
    private readonly IProcessEventBuffer _eventBuffer = eventBuffer;
    private readonly IAgentCredentialStore _credentialStore = credentialStore;
    private readonly IAgentEventClient _eventClient = eventClient;
    private readonly ProcessEventTransmissionOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var processEvent in _eventBuffer.ReadAllAsync(stoppingToken))
        {
            await DeliverAsync(processEvent, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBuffer.Complete();
        return base.StopAsync(cancellationToken);
    }

    private async Task DeliverAsync(ProcessTelemetryEvent processEvent, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var credentialLoadResult = await _credentialStore.LoadAsync(cancellationToken);

            if (credentialLoadResult.Status != AgentCredentialLoadStatus.Available)
            {
                _logger.LogWarning(
                    "Process event delivery is waiting for valid agent credentials. CredentialStatus: {CredentialStatus}",
                    credentialLoadResult.Status);
                await DelayBeforeRetry(cancellationToken);
                continue;
            }

            var result = await _eventClient.SendAsync(
                credentialLoadResult.Credentials!,
                processEvent,
                cancellationToken);

            if (result.Succeeded)
            {
                _logger.LogDebug(
                    "Process event transmitted. EventType: {EventType}; ProcessId: {ProcessId}",
                    processEvent.EventType,
                    processEvent.ProcessId);
                return;
            }

            if (result.Failure == AgentEventTransmissionFailure.InvalidPayload)
            {
                _logger.LogWarning(
                    "Process event was rejected as invalid and will be discarded. EventType: {EventType}; ProcessId: {ProcessId}",
                    processEvent.EventType,
                    processEvent.ProcessId);
                return;
            }

            _logger.LogWarning(
                "Process event was not delivered and will be retried. EventType: {EventType}; ProcessId: {ProcessId}; Reason: {Reason}",
                processEvent.EventType,
                processEvent.ProcessId,
                result.Failure);
            await DelayBeforeRetry(cancellationToken);
        }
    }

    private Task DelayBeforeRetry(CancellationToken cancellationToken) =>
        Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds), cancellationToken);
}
