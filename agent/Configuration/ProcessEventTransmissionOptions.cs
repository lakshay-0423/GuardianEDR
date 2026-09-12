using System.ComponentModel.DataAnnotations;

namespace Guardian.Agent.Configuration;

public sealed class ProcessEventTransmissionOptions
{
    public const string SectionName = "ProcessEventTransmission";

    [Range(1, 10_000)]
    public int QueueCapacity { get; init; } = 500;

    [Range(1, 300)]
    public int RetryDelaySeconds { get; init; } = 10;
}
