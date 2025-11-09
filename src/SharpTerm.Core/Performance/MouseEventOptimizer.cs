namespace SharpTerm.Core.Performance;

/// <summary>
/// Optimizes mouse event processing by filtering and debouncing rapid events.
/// </summary>
public class MouseEventOptimizer
{
    private DateTime _lastMoveTime;
    private (int x, int y) _lastMovePosition;
    private readonly TimeSpan _moveDebounceInterval;
    private readonly int _moveDistanceThreshold;
    private readonly Queue<MouseEventData> _eventQueue = new();

    public MouseEventOptimizer(TimeSpan? moveDebounceInterval = null, int moveDistanceThreshold = 1)
    {
        _moveDebounceInterval = moveDebounceInterval ?? TimeSpan.FromMilliseconds(16); // ~60 FPS
        _moveDistanceThreshold = moveDistanceThreshold;
    }

    /// <summary>
    /// Processes a mouse event and determines if it should be dispatched.
    /// </summary>
    public bool ShouldProcess(MouseEventData eventData)
    {
        if (eventData.Type == MouseEventType.Move)
        {
            return ShouldProcessMove(eventData);
        }

        // Process all non-move events immediately
        return true;
    }

    /// <summary>
    /// Enqueues a mouse event for batched processing.
    /// </summary>
    public void EnqueueEvent(MouseEventData eventData)
    {
        _eventQueue.Enqueue(eventData);
    }

    /// <summary>
    /// Dequeues and processes batched events.
    /// </summary>
    public IEnumerable<MouseEventData> ProcessBatchedEvents()
    {
        var processedEvents = new List<MouseEventData>();

        while (_eventQueue.Count > 0)
        {
            var evt = _eventQueue.Dequeue();

            if (evt.Type == MouseEventType.Move)
            {
                // Coalesce multiple move events
                if (_eventQueue.Count > 0 && _eventQueue.Peek().Type == MouseEventType.Move)
                {
                    continue; // Skip this move, use the next one
                }
            }

            if (ShouldProcess(evt))
            {
                processedEvents.Add(evt);
            }
        }

        return processedEvents;
    }

    /// <summary>
    /// Gets the number of queued events.
    /// </summary>
    public int QueuedEventCount => _eventQueue.Count;

    private bool ShouldProcessMove(MouseEventData eventData)
    {
        var now = DateTime.UtcNow;
        var timeSinceLastMove = now - _lastMoveTime;

        // Check time debounce
        if (timeSinceLastMove < _moveDebounceInterval)
        {
            return false;
        }

        // Check distance threshold
        var distance = Math.Abs(eventData.X - _lastMovePosition.x) +
                      Math.Abs(eventData.Y - _lastMovePosition.y);

        if (distance < _moveDistanceThreshold)
        {
            return false;
        }

        _lastMoveTime = now;
        _lastMovePosition = (eventData.X, eventData.Y);
        return true;
    }
}

/// <summary>
/// Mouse event data.
/// </summary>
public readonly struct MouseEventData
{
    public int X { get; }
    public int Y { get; }
    public MouseEventType Type { get; }
    public MouseButton Button { get; }
    public DateTime Timestamp { get; }

    public MouseEventData(int x, int y, MouseEventType type, MouseButton button)
    {
        X = x;
        Y = y;
        Type = type;
        Button = button;
        Timestamp = DateTime.UtcNow;
    }
}

public enum MouseEventType
{
    Move,
    Click,
    DoubleClick,
    Down,
    Up,
    Scroll
}

public enum MouseButton
{
    None,
    Left,
    Right,
    Middle,
    WheelUp,
    WheelDown
}

/// <summary>
/// Mouse event statistics tracker.
/// </summary>
public class MouseEventStatistics
{
    private int _totalEvents;
    private int _filteredEvents;
    private readonly Dictionary<MouseEventType, int> _eventCounts = new();

    public void RecordEvent(MouseEventData eventData, bool filtered)
    {
        _totalEvents++;
        if (filtered)
            _filteredEvents++;

        if (!_eventCounts.ContainsKey(eventData.Type))
        {
            _eventCounts[eventData.Type] = 0;
        }
        _eventCounts[eventData.Type]++;
    }

    public int TotalEvents => _totalEvents;
    public int FilteredEvents => _filteredEvents;
    public int ProcessedEvents => _totalEvents - _filteredEvents;
    public double FilterRatio => _totalEvents > 0 ? (double)_filteredEvents / _totalEvents : 0;

    public Dictionary<MouseEventType, int> EventCounts => new(_eventCounts);

    public void Reset()
    {
        _totalEvents = 0;
        _filteredEvents = 0;
        _eventCounts.Clear();
    }

    public override string ToString()
    {
        return $"Total: {TotalEvents}, Processed: {ProcessedEvents}, Filtered: {FilteredEvents} ({FilterRatio:P1})";
    }
}
