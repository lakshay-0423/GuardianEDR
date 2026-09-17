using System.ComponentModel.DataAnnotations;

namespace Guardian.Agent.Configuration;

public sealed class FileMonitoringOptions
{
    public const string SectionName = "FileMonitoring";

    public bool Enabled { get; init; } = true;

    public bool MonitorDownloads { get; init; } = true;

    public bool MonitorDesktop { get; init; } = true;

    public bool MonitorTemporaryDirectory { get; init; } = true;

    public bool IncludeSubdirectories { get; init; }

    [Range(0, 5_000)]
    public int DuplicateWindowMilliseconds { get; init; } = 750;

    [Range(4_096, 65_536)]
    public int InternalBufferSize { get; init; } = 32_768;

    [Range(1, 32)]
    public int MaximumDirectories { get; init; } = 16;

    public List<string> AdditionalPaths { get; init; } = [];
}
