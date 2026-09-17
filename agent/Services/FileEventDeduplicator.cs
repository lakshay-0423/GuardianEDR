using System.Collections.Concurrent;
using Guardian.Agent.Models;

namespace Guardian.Agent.Services;

public sealed class FileEventDeduplicator(TimeSpan duplicateWindow)
{
    private const int MaximumCachedEvents = 10_000;

    private readonly ConcurrentDictionary<string, DateTimeOffset> _recentEvents = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _duplicateWindow = duplicateWindow;
    private long _eventCount;

    public bool ShouldPublish(FileTelemetryEvent fileEvent)
    {
        if (_duplicateWindow == TimeSpan.Zero)
        {
            return true;
        }

        var now = DateTimeOffset.UtcNow;
        var key = $"{fileEvent.EventType}|{fileEvent.FullPath}|{fileEvent.PreviousPath}";

        if (_recentEvents.TryGetValue(key, out var previousTimestamp)
            && now - previousTimestamp < _duplicateWindow)
        {
            return false;
        }

        _recentEvents[key] = now;

        if (Interlocked.Increment(ref _eventCount) % 256 == 0 || _recentEvents.Count > MaximumCachedEvents)
        {
            RemoveExpiredEntries(now);
        }

        return true;
    }

    public void Clear() => _recentEvents.Clear();

    private void RemoveExpiredEntries(DateTimeOffset now)
    {
        var expiry = now - _duplicateWindow;

        foreach (var entry in _recentEvents.Where(entry => entry.Value < expiry))
        {
            _recentEvents.TryRemove(entry.Key, out _);
        }

        if (_recentEvents.Count <= MaximumCachedEvents)
        {
            return;
        }

        foreach (var entry in _recentEvents.OrderBy(entry => entry.Value).Take(_recentEvents.Count - MaximumCachedEvents))
        {
            _recentEvents.TryRemove(entry.Key, out _);
        }
    }
}
