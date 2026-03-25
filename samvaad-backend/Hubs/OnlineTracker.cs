using System.Collections.Concurrent;

namespace samvaad_backend.Hubs;

/// <summary>
/// Tracks which users are online and via how many connections.
/// Thread-safe singleton.
/// </summary>
public interface IOnlineTracker
{
    void SetOnline(Guid userId, string connectionId);

    /// <returns>true if the user still has at least one active connection</returns>
    bool SetOffline(Guid userId, string connectionId);

    bool IsOnline(Guid userId);
    IReadOnlySet<Guid> GetOnlineUserIds();
}

public sealed class OnlineTracker : IOnlineTracker
{
    // userId → set of active connection IDs
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    public void SetOnline(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out var set))
            {
                set = [];
                _connections[userId] = set;
            }
            set.Add(connectionId);
        }
    }

    public bool SetOffline(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out var set)) return false;
            set.Remove(connectionId);
            if (set.Count == 0)
            {
                _connections.TryRemove(userId, out _);
                return false;
            }
            return true;
        }
    }

    public bool IsOnline(Guid userId) => _connections.ContainsKey(userId);

    public IReadOnlySet<Guid> GetOnlineUserIds()
    {
        lock (_lock)
            return new HashSet<Guid>(_connections.Keys);
    }
}
