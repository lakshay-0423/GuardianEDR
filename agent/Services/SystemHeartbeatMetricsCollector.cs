using System.Runtime.InteropServices;
using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public sealed class SystemHeartbeatMetricsCollector(
    ILogger<SystemHeartbeatMetricsCollector> logger) : IHeartbeatMetricsCollector
{
    private readonly ILogger<SystemHeartbeatMetricsCollector> _logger = logger;
    private SystemCpuSample? _previousCpuSample;

    public HeartbeatMetrics Collect() => new(
        CpuUsage: GetCpuUsage(),
        MemoryUsage: GetMemoryUsage());

    private double GetCpuUsage()
    {
        try
        {
            if (!GetSystemTimes(out var idleTime, out var kernelTime, out var userTime))
            {
                return 0;
            }

            var currentSample = new SystemCpuSample(
                idleTime.ToLong(),
                kernelTime.ToLong(),
                userTime.ToLong());

            if (_previousCpuSample is not { } previousSample)
            {
                _previousCpuSample = currentSample;
                return 0;
            }

            _previousCpuSample = currentSample;
            var totalTime = (currentSample.KernelTime - previousSample.KernelTime)
                + (currentSample.UserTime - previousSample.UserTime);

            if (totalTime <= 0)
            {
                return 0;
            }

            var busyTime = totalTime - (currentSample.IdleTime - previousSample.IdleTime);
            return Math.Clamp(100d * busyTime / totalTime, 0, 100);
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            _logger.LogDebug("Windows system CPU usage is unavailable.");
            return 0;
        }
    }

    private double GetMemoryUsage()
    {
        try
        {
            var memoryStatus = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };

            if (!GlobalMemoryStatusEx(ref memoryStatus))
            {
                return 0;
            }

            return Math.Clamp(memoryStatus.MemoryLoad, 0, 100);
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            _logger.LogDebug("Windows system memory usage is unavailable.");
            return 0;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(
        out FileTime idleTime,
        out FileTime kernelTime,
        out FileTime userTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx memoryStatus);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct FileTime(uint lowDateTime, uint highDateTime)
    {
        private readonly uint _lowDateTime = lowDateTime;
        private readonly uint _highDateTime = highDateTime;

        public long ToLong() => ((long)_highDateTime << 32) | _lowDateTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    private readonly record struct SystemCpuSample(long IdleTime, long KernelTime, long UserTime);
}
