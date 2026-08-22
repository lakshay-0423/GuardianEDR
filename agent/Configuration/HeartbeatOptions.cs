using System.ComponentModel.DataAnnotations;

namespace Guardian.Agent.Configuration;

public sealed class HeartbeatOptions
{
    public const string SectionName = "Heartbeat";

    [Range(30, 86400)]
    public int FallbackIntervalSeconds { get; init; } = 300;
}
