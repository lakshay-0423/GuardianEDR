namespace Guardian.Agent.Models;

public sealed record AgentCredentialLoadResult(
    AgentCredentialLoadStatus Status,
    AgentCredentials? Credentials = null)
{
    public static AgentCredentialLoadResult Available(AgentCredentials credentials) =>
        new(AgentCredentialLoadStatus.Available, credentials);

    public static AgentCredentialLoadResult Missing() =>
        new(AgentCredentialLoadStatus.Missing);

    public static AgentCredentialLoadResult Corrupted() =>
        new(AgentCredentialLoadStatus.Corrupted);

    public static AgentCredentialLoadResult Unavailable() =>
        new(AgentCredentialLoadStatus.Unavailable);
}

public enum AgentCredentialLoadStatus
{
    Available,
    Missing,
    Corrupted,
    Unavailable,
}
