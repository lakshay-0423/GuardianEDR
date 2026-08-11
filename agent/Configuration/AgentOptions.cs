using System.ComponentModel.DataAnnotations;

namespace Guardian.Agent.Configuration;

public sealed class AgentOptions
{
    public const string SectionName = "Agent";

    [Required]
    [MinLength(1)]
    public string ServiceName { get; init; } = string.Empty;

    [Range(1, 300)]
    public int ShutdownTimeoutSeconds { get; init; } = 30;
}
