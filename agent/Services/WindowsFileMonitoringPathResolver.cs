using System.Runtime.InteropServices;
using Guardian.Agent.Configuration;

namespace Guardian.Agent.Services;

public sealed class WindowsFileMonitoringPathResolver : IFileMonitoringPathResolver
{
    private static readonly Guid DownloadsFolderId = new("374DE290-123F-4565-9164-39C4925E467B");

    public IReadOnlyList<string> Resolve(FileMonitoringOptions options)
    {
        var paths = new List<string>();

        if (options.MonitorDownloads && TryGetDownloadsDirectory(out var downloadsDirectory))
        {
            paths.Add(downloadsDirectory);
        }

        if (options.MonitorDesktop)
        {
            paths.Add(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        }

        if (options.MonitorTemporaryDirectory)
        {
            paths.Add(Path.GetTempPath());
        }

        paths.AddRange(options.AdditionalPaths.Select(Environment.ExpandEnvironmentVariables));

        return paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(NormalizePath)
            .Where(path => path is not null)
            .Select(path => path!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? NormalizePath(string path)
    {
        try
        {
            return Path.GetFullPath(path.Trim());
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or NotSupportedException
            or PathTooLongException)
        {
            return null;
        }
    }

    private static bool TryGetDownloadsDirectory(out string path)
    {
        path = string.Empty;
        var result = SHGetKnownFolderPath(DownloadsFolderId, 0, IntPtr.Zero, out var pathPointer);

        if (result != 0 || pathPointer == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            path = Marshal.PtrToStringUni(pathPointer) ?? string.Empty;
            return !string.IsNullOrWhiteSpace(path);
        }
        finally
        {
            Marshal.FreeCoTaskMem(pathPointer);
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)] Guid knownFolderId,
        uint flags,
        IntPtr token,
        out IntPtr path);
}
