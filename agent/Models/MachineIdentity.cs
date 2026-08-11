namespace Guardian.Agent.Models;

public sealed record MachineIdentity(
    string Hostname,
    string OperatingSystem,
    string Architecture,
    string Username);
