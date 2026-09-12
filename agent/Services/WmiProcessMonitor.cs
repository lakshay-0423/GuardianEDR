using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using Guardian.Agent.Configuration;
using Guardian.Agent.Models;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class WmiProcessMonitor(
    ILogger<WmiProcessMonitor> logger,
    IOptions<ProcessMonitoringOptions> processMonitoringOptions) : IProcessMonitor
{
    private const string WmiNamespace = @"root\CIMV2";
    private const int MaximumCachedProcesses = 10_000;

    private readonly ILogger<WmiProcessMonitor> _logger = logger;
    private readonly ProcessMonitoringOptions _processMonitoringOptions = processMonitoringOptions.Value;
    private readonly ConcurrentDictionary<int, ProcessTelemetryEvent> _startedProcesses = new();
    private readonly object _syncRoot = new();
    private ManagementEventWatcher? _processStartedWatcher;
    private ManagementEventWatcher? _processTerminatedWatcher;
    private Timer? _fallbackTimer;
    private Dictionary<int, FallbackProcessState> _fallbackProcesses = [];
    private int _isFallbackScanRunning;
    private bool _isStarted;
    private bool _isDisposed;

    public event EventHandler<ProcessTelemetryEvent>? ProcessEventReceived;

    public void Start()
    {
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            if (_isStarted)
            {
                return;
            }

            _processStartedWatcher = CreateWatcher("SELECT * FROM Win32_ProcessStartTrace");
            _processTerminatedWatcher = CreateWatcher("SELECT * FROM Win32_ProcessStopTrace");
            _processStartedWatcher.EventArrived += OnProcessStarted;
            _processTerminatedWatcher.EventArrived += OnProcessTerminated;

            try
            {
                _processStartedWatcher.Start();
                _processTerminatedWatcher.Start();
                _isStarted = true;
            }
            catch (Exception exception) when (exception is ManagementException or UnauthorizedAccessException)
            {
                DisposeWatchers();
                StartFallbackMonitor();
                _isStarted = true;
                _logger.LogWarning(
                    "WMI process event subscriptions are unavailable. Using low-frequency process monitoring fallback. ErrorType: {ErrorType}; ScanIntervalSeconds: {ScanIntervalSeconds}",
                    exception.GetType().Name,
                    _processMonitoringOptions.FallbackScanIntervalSeconds);
            }
        }
    }

    public void Stop()
    {
        lock (_syncRoot)
        {
            if (!_isStarted)
            {
                return;
            }

            DisposeWatchers();
            _fallbackTimer?.Dispose();
            _fallbackTimer = null;
            _fallbackProcesses.Clear();
            _isStarted = false;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Stop();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    private static ManagementEventWatcher CreateWatcher(string query) => new(
        new ManagementScope(WmiNamespace),
        new WqlEventQuery(query));

    private void OnProcessStarted(object sender, EventArrivedEventArgs eventArgs)
    {
        if (!TryGetProcessId(eventArgs.NewEvent, out var processId))
        {
            return;
        }

        try
        {
            var processEvent = BuildProcessEvent(
                ProcessEventType.Started,
                processId,
                GetStringProperty(eventArgs.NewEvent, "ProcessName"));
            _startedProcesses[processId] = processEvent;
            TrimCache();
            Publish(processEvent);
        }
        catch (Exception exception) when (
            exception is ManagementException
            or UnauthorizedAccessException
            or InvalidOperationException
            or Win32Exception)
        {
            _logger.LogWarning(
                "Process start metadata could not be collected. ProcessId: {ProcessId}; ErrorType: {ErrorType}",
                processId,
                exception.GetType().Name);
        }
    }

    private void OnProcessTerminated(object sender, EventArrivedEventArgs eventArgs)
    {
        if (!TryGetProcessId(eventArgs.NewEvent, out var processId))
        {
            return;
        }

        try
        {
            var processEvent = _startedProcesses.TryRemove(processId, out var startedProcess)
                ? startedProcess with
                {
                    EventType = ProcessEventType.Terminated,
                    Timestamp = DateTimeOffset.UtcNow,
                }
                : new ProcessTelemetryEvent(
                    ProcessEventType.Terminated,
                    processId,
                    GetStringProperty(eventArgs.NewEvent, "ProcessName"),
                    null,
                    null,
                    null,
                    null,
                    null,
                    DateTimeOffset.UtcNow);

            Publish(processEvent);
        }
        catch (Exception exception) when (exception is ManagementException or UnauthorizedAccessException)
        {
            _logger.LogWarning(
                "Process termination telemetry could not be collected. ProcessId: {ProcessId}; ErrorType: {ErrorType}",
                processId,
                exception.GetType().Name);
        }
    }

    private ProcessTelemetryEvent BuildProcessEvent(
        ProcessEventType eventType,
        int processId,
        string? traceProcessName)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                WmiNamespace,
                $"SELECT ProcessId, Name, ExecutablePath, ParentProcessId, CommandLine FROM Win32_Process WHERE ProcessId = {processId}");
            using var results = searcher.Get();
            using var process = results.Cast<ManagementObject>().FirstOrDefault();

            if (process is null)
            {
                return CreateBasicProcessEvent(eventType, processId, traceProcessName);
            }

            var parentProcessId = GetNullableProcessId(process["ParentProcessId"]);

            return new ProcessTelemetryEvent(
                eventType,
                processId,
                GetStringProperty(process, "Name") ?? traceProcessName,
                GetStringProperty(process, "ExecutablePath"),
                parentProcessId,
                parentProcessId is { } id ? GetProcessName(id) : null,
                GetStringProperty(process, "CommandLine"),
                GetOwner(process),
                DateTimeOffset.UtcNow);
        }
        catch (Exception exception) when (
            exception is ManagementException
            or UnauthorizedAccessException
            or InvalidOperationException
            or Win32Exception)
        {
            _logger.LogDebug(
                "Process metadata is unavailable. ProcessId: {ProcessId}; ErrorType: {ErrorType}",
                processId,
                exception.GetType().Name);
            return CreateBasicProcessEvent(eventType, processId, traceProcessName);
        }
    }

    private static ProcessTelemetryEvent CreateBasicProcessEvent(
        ProcessEventType eventType,
        int processId,
        string? processName) => new(
        eventType,
        processId,
        processName,
        null,
        null,
        null,
        null,
        null,
        DateTimeOffset.UtcNow);

    private void StartFallbackMonitor()
    {
        _fallbackProcesses = GetProcessSnapshot()
            .ToDictionary(
                entry => entry.Key,
                entry => new FallbackProcessState(
                    new ProcessTelemetryEvent(
                        ProcessEventType.Started,
                        entry.Key,
                        entry.Value,
                        null,
                        null,
                        null,
                        null,
                        null,
                        DateTimeOffset.UtcNow),
                    ReportTermination: false));

        var interval = TimeSpan.FromSeconds(_processMonitoringOptions.FallbackScanIntervalSeconds);
        _fallbackTimer = new Timer(
            _ => ScanFallbackProcesses(),
            state: null,
            dueTime: interval,
            period: interval);
    }

    private void ScanFallbackProcesses()
    {
        if (Interlocked.Exchange(ref _isFallbackScanRunning, 1) == 1)
        {
            return;
        }

        try
        {
            var currentProcesses = GetProcessSnapshot();

            lock (_syncRoot)
            {
                foreach (var process in currentProcesses.Where(process => !_fallbackProcesses.ContainsKey(process.Key)))
                {
                    var processEvent = BuildProcessEvent(ProcessEventType.Started, process.Key, process.Value);
                    _fallbackProcesses[process.Key] = new FallbackProcessState(processEvent, ReportTermination: true);
                    Publish(processEvent);
                }

                foreach (var process in _fallbackProcesses.Where(process => !currentProcesses.ContainsKey(process.Key)).ToArray())
                {
                    _fallbackProcesses.Remove(process.Key);

                    if (process.Value.ReportTermination)
                    {
                        Publish(process.Value.Event with
                        {
                            EventType = ProcessEventType.Terminated,
                            Timestamp = DateTimeOffset.UtcNow,
                        });
                    }
                }
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException or Win32Exception or ManagementException)
        {
            _logger.LogWarning(
                "Fallback process monitoring scan failed. ErrorType: {ErrorType}",
                exception.GetType().Name);
        }
        finally
        {
            Volatile.Write(ref _isFallbackScanRunning, 0);
        }
    }

    private static Dictionary<int, string?> GetProcessSnapshot()
    {
        var processSnapshot = new Dictionary<int, string?>();

        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    processSnapshot[process.Id] = process.ProcessName;
                }
                catch (Exception exception) when (exception is InvalidOperationException or Win32Exception)
                {
                    // A process can terminate between enumeration and metadata access.
                }
            }
        }

        return processSnapshot;
    }

    private static string? GetOwner(ManagementObject process)
    {
        try
        {
            using var owner = process.InvokeMethod("GetOwner", null, null);
            var username = owner?["User"]?.ToString();
            var domain = owner?["Domain"]?.ToString();

            return string.IsNullOrWhiteSpace(username)
                ? null
                : string.IsNullOrWhiteSpace(domain)
                    ? username
                    : $"{domain}\\{username}";
        }
        catch (Exception exception) when (
            exception is ManagementException
            or UnauthorizedAccessException
            or InvalidOperationException
            or Win32Exception)
        {
            return null;
        }
    }

    private static string? GetProcessName(int processId)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                WmiNamespace,
                $"SELECT Name FROM Win32_Process WHERE ProcessId = {processId}");
            using var results = searcher.Get();
            using var process = results.Cast<ManagementObject>().FirstOrDefault();

            return process is null ? null : GetStringProperty(process, "Name");
        }
        catch (Exception exception) when (
            exception is ManagementException
            or UnauthorizedAccessException
            or InvalidOperationException
            or Win32Exception)
        {
            return null;
        }
    }

    private static bool TryGetProcessId(ManagementBaseObject traceEvent, out int processId)
    {
        processId = GetNullableProcessId(traceEvent["ProcessID"]) ?? 0;
        return processId > 0;
    }

    private static int? GetNullableProcessId(object? value) => value switch
    {
        uint processId when processId <= int.MaxValue => (int)processId,
        int processId when processId > 0 => processId,
        _ => null,
    };

    private static string? GetStringProperty(ManagementBaseObject managementObject, string propertyName) =>
        managementObject[propertyName]?.ToString();

    private void Publish(ProcessTelemetryEvent processEvent) =>
        ProcessEventReceived?.Invoke(this, processEvent);

    private void TrimCache()
    {
        if (_startedProcesses.Count < MaximumCachedProcesses)
        {
            return;
        }

        var cutoff = DateTimeOffset.UtcNow.AddHours(-24);

        foreach (var entry in _startedProcesses.Where(entry => entry.Value.Timestamp < cutoff))
        {
            _startedProcesses.TryRemove(entry.Key, out _);
        }
    }

    private void DisposeWatchers()
    {
        if (_processStartedWatcher is not null)
        {
            _processStartedWatcher.EventArrived -= OnProcessStarted;
            try
            {
                _processStartedWatcher.Stop();
            }
            catch (ManagementException)
            {
                // The watcher may not have completed startup.
            }
            _processStartedWatcher.Dispose();
            _processStartedWatcher = null;
        }

        if (_processTerminatedWatcher is not null)
        {
            _processTerminatedWatcher.EventArrived -= OnProcessTerminated;
            try
            {
                _processTerminatedWatcher.Stop();
            }
            catch (ManagementException)
            {
                // The watcher may not have completed startup.
            }
            _processTerminatedWatcher.Dispose();
            _processTerminatedWatcher = null;
        }
    }

    private sealed record FallbackProcessState(ProcessTelemetryEvent Event, bool ReportTermination);
}
