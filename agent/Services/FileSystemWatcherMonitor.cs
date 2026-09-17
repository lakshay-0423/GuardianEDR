using Guardian.Agent.Configuration;
using Guardian.Agent.Models;
using Microsoft.Extensions.Options;

namespace Guardian.Agent.Services;

public sealed class FileSystemWatcherMonitor(
    ILogger<FileSystemWatcherMonitor> logger,
    IFileMonitoringPathResolver pathResolver,
    IOptions<FileMonitoringOptions> fileMonitoringOptions) : IFileMonitor
{
    private readonly ILogger<FileSystemWatcherMonitor> _logger = logger;
    private readonly IFileMonitoringPathResolver _pathResolver = pathResolver;
    private readonly FileMonitoringOptions _options = fileMonitoringOptions.Value;
    private readonly FileEventDeduplicator _deduplicator = new(
        TimeSpan.FromMilliseconds(fileMonitoringOptions.Value.DuplicateWindowMilliseconds));
    private readonly object _syncRoot = new();
    private readonly List<FileSystemWatcher> _watchers = [];
    private bool _isDisposed;
    private bool _isStarted;

    public event EventHandler<FileTelemetryEvent>? FileEventReceived;

    public void Start()
    {
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            if (_isStarted)
            {
                return;
            }

            var paths = _pathResolver.Resolve(_options);

            if (paths.Count > _options.MaximumDirectories)
            {
                _logger.LogWarning(
                    "Configured file monitoring paths exceed the allowed limit. ConfiguredCount: {ConfiguredCount}; MaximumDirectories: {MaximumDirectories}",
                    paths.Count,
                    _options.MaximumDirectories);
            }

            foreach (var path in paths.Take(_options.MaximumDirectories))
            {
                StartWatcher(path);
            }

            _isStarted = true;
            _logger.LogInformation("Windows file monitoring started. ActiveDirectoryCount: {ActiveDirectoryCount}", _watchers.Count);
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

            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Created -= OnCreated;
                watcher.Changed -= OnChanged;
                watcher.Deleted -= OnDeleted;
                watcher.Renamed -= OnRenamed;
                watcher.Error -= OnError;
                watcher.Dispose();
            }

            _watchers.Clear();
            _deduplicator.Clear();
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

    private void StartWatcher(string path)
    {
        if (!Directory.Exists(path))
        {
            _logger.LogWarning("Configured file monitoring path is unavailable. Path: {Path}", path);
            return;
        }

        FileSystemWatcher? watcher = null;

        try
        {
            watcher = new FileSystemWatcher(path)
            {
                Filter = "*",
                IncludeSubdirectories = _options.IncludeSubdirectories,
                InternalBufferSize = _options.InternalBufferSize,
                NotifyFilter = NotifyFilters.FileName
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Size
                    | NotifyFilters.CreationTime,
            };
            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;
            watcher.EnableRaisingEvents = true;
            _watchers.Add(watcher);
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or IOException
            or UnauthorizedAccessException)
        {
            watcher?.Dispose();
            _logger.LogWarning(
                "Configured file monitoring path could not be watched. Path: {Path}; ErrorType: {ErrorType}",
                path,
                exception.GetType().Name);
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs eventArgs) =>
        HandleFileEvent(FileEventType.Created, eventArgs.FullPath, previousPath: null);

    private void OnChanged(object sender, FileSystemEventArgs eventArgs) =>
        HandleFileEvent(FileEventType.Modified, eventArgs.FullPath, previousPath: null);

    private void OnDeleted(object sender, FileSystemEventArgs eventArgs) =>
        HandleFileEvent(FileEventType.Deleted, eventArgs.FullPath, previousPath: null);

    private void OnRenamed(object sender, RenamedEventArgs eventArgs) =>
        HandleFileEvent(FileEventType.Renamed, eventArgs.FullPath, eventArgs.OldFullPath);

    private void OnError(object sender, ErrorEventArgs eventArgs)
    {
        var exception = eventArgs.GetException();
        _logger.LogWarning(
            "File monitoring watcher reported an error. ErrorType: {ErrorType}; IsBufferOverflow: {IsBufferOverflow}",
            exception.GetType().Name,
            exception is InternalBufferOverflowException);
    }

    private void HandleFileEvent(FileEventType eventType, string fullPath, string? previousPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return;
        }

        var fileEvent = new FileTelemetryEvent(
            eventType,
            fullPath,
            Path.GetFileName(fullPath),
            Path.GetExtension(fullPath),
            DateTimeOffset.UtcNow,
            eventType == FileEventType.Deleted ? null : TryGetFileSize(fullPath),
            previousPath);

        if (_deduplicator.ShouldPublish(fileEvent))
        {
            FileEventReceived?.Invoke(this, fileEvent);
        }
    }

    private static long? TryGetFileSize(string fullPath)
    {
        try
        {
            return new FileInfo(fullPath).Length;
        }
        catch (Exception exception) when (
            exception is IOException
            or UnauthorizedAccessException
            or NotSupportedException)
        {
            return null;
        }
    }
}
