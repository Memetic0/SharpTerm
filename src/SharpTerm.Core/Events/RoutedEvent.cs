namespace SharpTerm.Core.Events;

/// <summary>
/// Represents a routed event that can bubble or tunnel through the widget tree.
/// </summary>
public class RoutedEvent
{
    public string Name { get; }
    public Type OwnerType { get; }
    public RoutingStrategy RoutingStrategy { get; }

    public RoutedEvent(string name, Type ownerType, RoutingStrategy routingStrategy)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        OwnerType = ownerType ?? throw new ArgumentNullException(nameof(ownerType));
        RoutingStrategy = routingStrategy;
    }
}

/// <summary>
/// Defines how an event routes through the widget tree.
/// </summary>
public enum RoutingStrategy
{
    /// <summary>
    /// Event starts at the source and bubbles up to the root.
    /// </summary>
    Bubble,

    /// <summary>
    /// Event starts at the root and tunnels down to the source.
    /// </summary>
    Tunnel,

    /// <summary>
    /// Event is raised only on the source widget.
    /// </summary>
    Direct
}

/// <summary>
/// Event arguments for routed events.
/// </summary>
public class RoutedEventArgs : EventArgs
{
    public RoutedEvent RoutedEvent { get; }
    public object? Source { get; set; }
    public bool Handled { get; set; }

    public RoutedEventArgs(RoutedEvent routedEvent, object? source = null)
    {
        RoutedEvent = routedEvent ?? throw new ArgumentNullException(nameof(routedEvent));
        Source = source;
    }
}

/// <summary>
/// Delegate for routed event handlers.
/// </summary>
public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

/// <summary>
/// Generic routed event arguments.
/// </summary>
public class RoutedEventArgs<T> : RoutedEventArgs
{
    public T? Data { get; set; }

    public RoutedEventArgs(RoutedEvent routedEvent, object? source = null, T? data = default)
        : base(routedEvent, source)
    {
        Data = data;
    }
}

/// <summary>
/// Manages routed event subscriptions and routing.
/// </summary>
public class RoutedEventManager
{
    private readonly Dictionary<RoutedEvent, List<(Widget widget, RoutedEventHandler handler)>> _handlers = new();

    /// <summary>
    /// Registers a routed event.
    /// </summary>
    public static RoutedEvent RegisterRoutedEvent(string name, Type ownerType, RoutingStrategy routingStrategy)
    {
        return new RoutedEvent(name, ownerType, routingStrategy);
    }

    /// <summary>
    /// Adds a handler for a routed event.
    /// </summary>
    public void AddHandler(Widget widget, RoutedEvent routedEvent, RoutedEventHandler handler)
    {
        if (widget == null)
            throw new ArgumentNullException(nameof(widget));
        if (routedEvent == null)
            throw new ArgumentNullException(nameof(routedEvent));
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        if (!_handlers.ContainsKey(routedEvent))
        {
            _handlers[routedEvent] = new List<(Widget, RoutedEventHandler)>();
        }

        _handlers[routedEvent].Add((widget, handler));
    }

    /// <summary>
    /// Removes a handler for a routed event.
    /// </summary>
    public void RemoveHandler(Widget widget, RoutedEvent routedEvent, RoutedEventHandler handler)
    {
        if (_handlers.TryGetValue(routedEvent, out var handlers))
        {
            handlers.RemoveAll(h => h.widget == widget && h.handler == handler);
        }
    }

    /// <summary>
    /// Raises a routed event.
    /// </summary>
    public void RaiseEvent(Widget source, RoutedEventArgs e)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (e == null)
            throw new ArgumentNullException(nameof(e));

        e.Source ??= source;

        switch (e.RoutedEvent.RoutingStrategy)
        {
            case RoutingStrategy.Direct:
                InvokeHandlers(source, e);
                break;

            case RoutingStrategy.Bubble:
                BubbleEvent(source, e);
                break;

            case RoutingStrategy.Tunnel:
                TunnelEvent(source, e);
                break;
        }
    }

    private void BubbleEvent(Widget source, RoutedEventArgs e)
    {
        var current = source;
        while (current != null && !e.Handled)
        {
            InvokeHandlers(current, e);
            current = current.Parent;
        }
    }

    private void TunnelEvent(Widget source, RoutedEventArgs e)
    {
        // Build path from root to source
        var path = new List<Widget>();
        var current = source;
        while (current != null)
        {
            path.Insert(0, current);
            current = current.Parent;
        }

        // Invoke handlers from root to source
        foreach (var widget in path)
        {
            if (e.Handled)
                break;
            InvokeHandlers(widget, e);
        }
    }

    private void InvokeHandlers(Widget widget, RoutedEventArgs e)
    {
        if (_handlers.TryGetValue(e.RoutedEvent, out var handlers))
        {
            foreach (var (handlerWidget, handler) in handlers.ToList())
            {
                if (handlerWidget == widget)
                {
                    handler(widget, e);
                    if (e.Handled)
                        break;
                }
            }
        }
    }
}

/// <summary>
/// Extension methods for routed events on widgets.
/// </summary>
public static class RoutedEventExtensions
{
    private static readonly Dictionary<Widget, RoutedEventManager> _managers = new();

    /// <summary>
    /// Gets or creates the routed event manager for a widget.
    /// </summary>
    private static RoutedEventManager GetManager(Widget widget)
    {
        if (!_managers.TryGetValue(widget, out var manager))
        {
            manager = new RoutedEventManager();
            _managers[widget] = manager;
        }
        return manager;
    }

    /// <summary>
    /// Adds a routed event handler to a widget.
    /// </summary>
    public static void AddHandler(this Widget widget, RoutedEvent routedEvent, RoutedEventHandler handler)
    {
        GetManager(widget).AddHandler(widget, routedEvent, handler);
    }

    /// <summary>
    /// Removes a routed event handler from a widget.
    /// </summary>
    public static void RemoveHandler(this Widget widget, RoutedEvent routedEvent, RoutedEventHandler handler)
    {
        GetManager(widget).RemoveHandler(widget, routedEvent, handler);
    }

    /// <summary>
    /// Raises a routed event.
    /// </summary>
    public static void RaiseEvent(this Widget widget, RoutedEventArgs e)
    {
        GetManager(widget).RaiseEvent(widget, e);
    }
}
