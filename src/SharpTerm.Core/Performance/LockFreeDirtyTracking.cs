using System.Collections.Concurrent;

namespace SharpTerm.Core.Performance;

/// <summary>
/// Lock-free dirty widget tracking using concurrent collections.
/// </summary>
public class LockFreeDirtyTracker
{
    private readonly ConcurrentBag<Widget> _dirtyWidgets = new();
    private readonly ConcurrentDictionary<Widget, byte> _dirtySet = new();

    /// <summary>
    /// Marks a widget as dirty (thread-safe, lock-free).
    /// </summary>
    public void MarkDirty(Widget widget)
    {
        if (_dirtySet.TryAdd(widget, 0))
        {
            _dirtyWidgets.Add(widget);
        }
    }

    /// <summary>
    /// Gets all dirty widgets and clears the tracker.
    /// </summary>
    public IEnumerable<Widget> GetAndClearDirtyWidgets()
    {
        var widgets = _dirtyWidgets.ToArray();

        // Clear the bag by creating a new one
        while (_dirtyWidgets.TryTake(out _)) { }

        _dirtySet.Clear();

        return widgets;
    }

    /// <summary>
    /// Checks if a widget is dirty.
    /// </summary>
    public bool IsDirty(Widget widget)
    {
        return _dirtySet.ContainsKey(widget);
    }

    /// <summary>
    /// Gets the number of dirty widgets (approximate).
    /// </summary>
    public int Count => _dirtySet.Count;
}

/// <summary>
/// Lock-free event queue for cross-thread event delivery.
/// </summary>
public class LockFreeEventQueue<TEvent>
{
    private readonly ConcurrentQueue<TEvent> _queue = new();

    /// <summary>
    /// Enqueues an event (thread-safe).
    /// </summary>
    public void Enqueue(TEvent eventData)
    {
        _queue.Enqueue(eventData);
    }

    /// <summary>
    /// Dequeues all pending events.
    /// </summary>
    public IEnumerable<TEvent> DequeueAll()
    {
        while (_queue.TryDequeue(out var eventData))
        {
            yield return eventData;
        }
    }

    /// <summary>
    /// Tries to dequeue a single event.
    /// </summary>
    public bool TryDequeue(out TEvent eventData)
    {
        return _queue.TryDequeue(out eventData!);
    }

    /// <summary>
    /// Gets the approximate count of pending events.
    /// </summary>
    public int Count => _queue.Count;

    /// <summary>
    /// Clears all pending events.
    /// </summary>
    public void Clear()
    {
        while (_queue.TryDequeue(out _)) { }
    }
}

/// <summary>
/// Partitioned dirty tracker for reduced contention in multi-threaded scenarios.
/// </summary>
public class PartitionedDirtyTracker
{
    private readonly LockFreeDirtyTracker[] _partitions;
    private readonly int _partitionCount;

    public PartitionedDirtyTracker(int partitionCount = 4)
    {
        _partitionCount = partitionCount;
        _partitions = new LockFreeDirtyTracker[partitionCount];

        for (int i = 0; i < partitionCount; i++)
        {
            _partitions[i] = new LockFreeDirtyTracker();
        }
    }

    /// <summary>
    /// Marks a widget as dirty in the appropriate partition.
    /// </summary>
    public void MarkDirty(Widget widget)
    {
        var partition = GetPartition(widget);
        partition.MarkDirty(widget);
    }

    /// <summary>
    /// Gets all dirty widgets from all partitions.
    /// </summary>
    public IEnumerable<Widget> GetAndClearDirtyWidgets()
    {
        for (int i = 0; i < _partitionCount; i++)
        {
            foreach (var widget in _partitions[i].GetAndClearDirtyWidgets())
            {
                yield return widget;
            }
        }
    }

    /// <summary>
    /// Gets the total number of dirty widgets across all partitions.
    /// </summary>
    public int Count => _partitions.Sum(p => p.Count);

    private LockFreeDirtyTracker GetPartition(Widget widget)
    {
        // Use hash code to distribute widgets across partitions
        var hash = widget.GetHashCode();
        var index = Math.Abs(hash % _partitionCount);
        return _partitions[index];
    }
}
