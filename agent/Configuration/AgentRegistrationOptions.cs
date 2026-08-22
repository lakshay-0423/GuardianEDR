using System.ComponentModel.DataAnnotations;

namespace Guardian.Agent.Configuration;

public sealed class AgentRegistrationOptions
{
    public const string SectionName = "Registration";

    [Required]
    [Url]
    public string BackendBaseUrl { get; init; } = string.Empty;

    [Range(1, 120)]
    public int TimeoutSeconds { get; init; } = 15;
}
