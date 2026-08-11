using Guardian.Agent.Configuration;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class AgentHostedService(
    ILogger<AgentHostedService> logger,
    IMachineIdentityProvider machineIdentityProvider,
    IOptions<AgentOptions> agentOptions) : IHostedService
{
    private readonly ILogger<AgentHostedService> _logger = logger;
    private readonly IMachineIdentityProvider _machineIdentityProvider = machineIdentityProvider;
    private readonly AgentOptions _agentOptions = agentOptions.Value;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var machine = _machineIdentityProvider.GetIdentity();

        _logger.LogInformation(
            "{ServiceName} initialized. Hostname: {Hostname}; OperatingSystem: {OperatingSystem}; Architecture: {Architecture}; Username: {Username}; ShutdownTimeoutSeconds: {ShutdownTimeoutSeconds}",
            _agentOptions.ServiceName,
            machine.Hostname,
            machine.OperatingSystem,
            machine.Architecture,
            machine.Username,
            _agentOptions.ShutdownTimeoutSeconds);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} is shutting down gracefully.", _agentOptions.ServiceName);
        return Task.CompletedTask;
    }
}
