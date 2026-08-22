namespace Guardian.Agent.Models;

public sealed record AgentRegistrationRequest(
    string Hostname,
    string OperatingSystem,
    string Architecture,
    string Username,
    string AgentVersion,
    Guid DeviceUuid);
