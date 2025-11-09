namespace SharpTerm.Core.Performance;

/// <summary>
/// Optimized event handler with cached invocation list.
/// </summary>
public class OptimizedEventHandler<TEventArgs> where TEventArgs : EventArgs
{
    private EventHandler<TEventArgs>? _handler;
    private Delegate[]? _cachedInvocationList;
    private int _invocationListVersion;

    /// <summary>
    /// Adds an event handler.
    /// </summary>
    public void Add(EventHandler<TEventArgs> handler)
    {
        _handler += handler;
        _cachedInvocationList = null; // Invalidate cache
        _invocationListVersion++;
    }

    /// <summary>
    /// Removes an event handler.
    /// </summary>
    public void Remove(EventHandler<TEventArgs> handler)
    {
        _handler -= handler;
        _cachedInvocationList = null; // Invalidate cache
        _invocationListVersion++;
    }

    /// <summary>
    /// Invokes all event handlers with caching.
    /// </summary>
    public void Invoke(object? sender, TEventArgs e)
    {
        if (_handler == null)
            return;

        // Cache invocation list to avoid allocation on every invoke
        _cachedInvocationList ??= _handler.GetInvocationList();

        foreach (var d in _cachedInvocationList)
        {
            ((EventHandler<TEventArgs>)d)(sender, e);
        }
    }

    /// <summary>
    /// Gets the number of subscribers.
    /// </summary>
    public int SubscriberCount => _cachedInvocationList?.Length ?? 0;
}

/// <summary>
/// Event aggregator for reducing event handler overhead.
/// </summary>
public class EventAggregator
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly object _lock = new();

    /// <summary>
    /// Subscribes to an event type.
    /// </summary>
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        lock (_lock)
        {
            var eventType = typeof(TEvent);
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }

            _subscribers[eventType].Add(handler);
        }
    }

    /// <summary>
    /// Unsubscribes from an event type.
    /// </summary>
    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        lock (_lock)
        {
            var eventType = typeof(TEvent);
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }

    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    public void Publish<TEvent>(TEvent eventData) where TEvent : class
    {
        List<Delegate>? handlers;

        lock (_lock)
        {
            var eventType = typeof(TEvent);
            if (!_subscribers.TryGetValue(eventType, out handlers))
                return;

            // Copy handlers to avoid lock during invocation
            handlers = new List<Delegate>(handlers);
        }

        foreach (var handler in handlers)
        {
            ((Action<TEvent>)handler)(eventData);
        }
    }

    /// <summary>
    /// Clears all subscriptions.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _subscribers.Clear();
        }
    }
}

/// <summary>
/// Weak event manager to prevent memory leaks from event subscriptions.
/// </summary>
public class WeakEventManager<TEventArgs> where TEventArgs : EventArgs
{
    private readonly List<WeakReference<EventHandler<TEventArgs>>> _handlers = new();

    /// <summary>
    /// Adds a weak event handler.
    /// </summary>
    public void AddHandler(EventHandler<TEventArgs> handler)
    {
        _handlers.Add(new WeakReference<EventHandler<TEventArgs>>(handler));
    }

    /// <summary>
    /// Removes a weak event handler.
    /// </summary>
    public void RemoveHandler(EventHandler<TEventArgs> handler)
    {
        _handlers.RemoveAll(wr =>
        {
            if (wr.TryGetTarget(out var target))
            {
                return target == handler;
            }
            return true; // Remove dead references
        });
    }

    /// <summary>
    /// Invokes all live event handlers.
    /// </summary>
    public void RaiseEvent(object? sender, TEventArgs e)
    {
        // Clean up dead references while invoking
        for (int i = _handlers.Count - 1; i >= 0; i--)
        {
            if (_handlers[i].TryGetTarget(out var handler))
            {
                handler(sender, e);
            }
            else
            {
                _handlers.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Gets the number of live handlers.
    /// </summary>
    public int LiveHandlerCount => _handlers.Count(wr => wr.TryGetTarget(out _));
}
