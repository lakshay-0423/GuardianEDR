namespace Guardian.Agent.Models;

public sealed record EndpointSystemInformation(
    string? Hostname,
    string? WindowsOsName,
    string? WindowsOsVersion,
    string? Architecture,
    string? CurrentUsername,
    string? AgentVersion,
    string? DeviceIdentifier,
    string? LocalIpAddress);
