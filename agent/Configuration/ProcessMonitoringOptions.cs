namespace Guardian.Agent.Configuration;

public sealed class ProcessMonitoringOptions
{
    public const string SectionName = "ProcessMonitoring";

    public bool Enabled { get; init; } = true;

    [System.ComponentModel.DataAnnotations.Range(1, 60)]
    public int FallbackScanIntervalSeconds { get; init; } = 5;
}
