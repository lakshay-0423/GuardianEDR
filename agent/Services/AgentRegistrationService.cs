using System.Security.Cryptography;
using Guardian.Agent.Communication;
using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public sealed class AgentRegistrationService(
    IAgentRegistrationClient registrationClient,
    IAgentCredentialStore credentialStore) : IAgentRegistrationService
{
    private readonly IAgentRegistrationClient _registrationClient = registrationClient;
    private readonly IAgentCredentialStore _credentialStore = credentialStore;

    public async Task<AgentRegistrationResult> RegisterAsync(
        EndpointSystemInformation systemInformation,
        CancellationToken cancellationToken)
    {
        if (!TryCreateRegistrationRequest(systemInformation, out var registrationRequest, out var validationFailure))
        {
            return AgentRegistrationResult.Failure(validationFailure);
        }

        var registrationResponse = await _registrationClient.RegisterAsync(registrationRequest, cancellationToken);

        if (!registrationResponse.Succeeded)
        {
            return AgentRegistrationResult.Failure(registrationResponse.FailureReason!);
        }

        try
        {
            await _credentialStore.StoreAsync(
                new AgentCredentials(
                    registrationResponse.AgentId!,
                    registrationResponse.AgentToken!,
                    registrationResponse.HeartbeatIntervalSeconds!.Value,
                    DateTimeOffset.UtcNow),
                cancellationToken);
        }
        catch (CryptographicException)
        {
            return AgentRegistrationResult.Failure("Windows could not protect agent credentials.");
        }
        catch (IOException)
        {
            return AgentRegistrationResult.Failure("Agent credentials could not be stored.");
        }
        catch (UnauthorizedAccessException)
        {
            return AgentRegistrationResult.Failure("Agent credentials could not be stored.");
        }

        return AgentRegistrationResult.Success(
            registrationResponse.AgentId!,
            registrationResponse.HeartbeatIntervalSeconds!.Value);
    }

    private static bool TryCreateRegistrationRequest(
        EndpointSystemInformation systemInformation,
        out AgentRegistrationRequest registrationRequest,
        out string failureReason)
    {
        if (!TryGetRequiredValue(systemInformation.Hostname, "hostname", 255, out var hostname, out failureReason)
            || !TryGetOperatingSystem(systemInformation, out var operatingSystem, out failureReason)
            || !TryGetRequiredValue(systemInformation.Architecture, "architecture", 50, out var architecture, out failureReason)
            || !TryGetRequiredValue(systemInformation.CurrentUsername, "current username", 255, out var username, out failureReason)
            || !TryGetRequiredValue(systemInformation.AgentVersion, "agent version", 100, out var agentVersion, out failureReason))
        {
            registrationRequest = default!;
            return false;
        }

        if (!Guid.TryParse(systemInformation.DeviceIdentifier, out var deviceUuid))
        {
            registrationRequest = default!;
            failureReason = "Device identifier is unavailable or invalid.";
            return false;
        }

        registrationRequest = new AgentRegistrationRequest(
            hostname,
            operatingSystem,
            architecture,
            username,
            agentVersion,
            deviceUuid);
        failureReason = string.Empty;
        return true;
    }

    private static bool TryGetOperatingSystem(
        EndpointSystemInformation systemInformation,
        out string operatingSystem,
        out string failureReason)
    {
        var operatingSystemDescription = string.Join(
            ' ',
            new[] { systemInformation.WindowsOsName, systemInformation.WindowsOsVersion }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim()));

        return TryGetRequiredValue(
            operatingSystemDescription,
            "Windows operating system",
            100,
            out operatingSystem,
            out failureReason);
    }

    private static bool TryGetRequiredValue(
        string? value,
        string fieldName,
        int maximumLength,
        out string normalizedValue,
        out string failureReason)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            normalizedValue = string.Empty;
            failureReason = $"Required system information is unavailable: {fieldName}.";
            return false;
        }

        normalizedValue = value.Trim();

        if (normalizedValue.Length <= maximumLength)
        {
            failureReason = string.Empty;
            return true;
        }

        normalizedValue = string.Empty;
        failureReason = $"Required system information exceeds the backend limit: {fieldName}.";
        return false;
    }
}
