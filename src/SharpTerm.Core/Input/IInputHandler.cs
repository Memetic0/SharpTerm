namespace SharpTerm.Core.Input;

/// <summary>
/// Handles input events and optionally passes them to the next handler.
/// </summary>
public interface IInputHandler
{
    /// <summary>
    /// Gets or sets the next handler in the chain.
    /// </summary>
    IInputHandler? Next { get; set; }

    /// <summary>
    /// Handles a keyboard event.
    /// </summary>
    /// <returns>True if the event was handled and should not be passed to the next handler.</returns>
    bool HandleKey(ConsoleKeyInfo keyInfo, InputContext context);

    /// <summary>
    /// Handles a mouse event.
    /// </summary>
    /// <returns>True if the event was handled and should not be passed to the next handler.</returns>
    bool HandleMouse(MouseEvent mouseEvent, InputContext context);
}

/// <summary>
/// Context information for input handling.
/// </summary>
public class InputContext
{
    public Widget? FocusedWidget { get; set; }
    public IEnumerable<Widget> Widgets { get; set; } = Enumerable.Empty<Widget>();
    public bool StopPropagation { get; set; }
    public Dictionary<string, object> Properties { get; } = new();
}

/// <summary>
/// Represents a mouse event.
/// </summary>
public class MouseEvent
{
    public int X { get; set; }
    public int Y { get; set; }
    public MouseEventType Type { get; set; }
    public MouseButton Button { get; set; }
}

public enum MouseEventType
{
    Click,
    DoubleClick,
    Move,
    Scroll,
    Down,
    Up
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
/// Base class for input handlers.
/// </summary>
public abstract class InputHandler : IInputHandler
{
    public IInputHandler? Next { get; set; }

    public virtual bool HandleKey(ConsoleKeyInfo keyInfo, InputContext context)
    {
        return Next?.HandleKey(keyInfo, context) ?? false;
    }

    public virtual bool HandleMouse(MouseEvent mouseEvent, InputContext context)
    {
        return Next?.HandleMouse(mouseEvent, context) ?? false;
    }
}

/// <summary>
/// Chains multiple input handlers together.
/// </summary>
public class InputHandlerChain : IInputHandler
{
    private IInputHandler? _firstHandler;
    private IInputHandler? _lastHandler;

    public IInputHandler? Next { get; set; }

    /// <summary>
    /// Adds a handler to the end of the chain.
    /// </summary>
    public InputHandlerChain Add(IInputHandler handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        if (_firstHandler == null)
        {
            _firstHandler = handler;
            _lastHandler = handler;
        }
        else
        {
            _lastHandler!.Next = handler;
            _lastHandler = handler;
        }

        return this;
    }

    public bool HandleKey(ConsoleKeyInfo keyInfo, InputContext context)
    {
        return _firstHandler?.HandleKey(keyInfo, context) ?? false;
    }

    public bool HandleMouse(MouseEvent mouseEvent, InputContext context)
    {
        return _firstHandler?.HandleMouse(mouseEvent, context) ?? false;
    }
}

/// <summary>
/// Handler that processes keyboard shortcuts.
/// </summary>
public class ShortcutHandler : InputHandler
{
    private readonly Dictionary<(ConsoleKey key, ConsoleModifiers modifiers), Action<InputContext>> _shortcuts = new();

    public void RegisterShortcut(ConsoleKey key, ConsoleModifiers modifiers, Action<InputContext> action)
    {
        _shortcuts[(key, modifiers)] = action ?? throw new ArgumentNullException(nameof(action));
    }

    public void UnregisterShortcut(ConsoleKey key, ConsoleModifiers modifiers)
    {
        _shortcuts.Remove((key, modifiers));
    }

    public override bool HandleKey(ConsoleKeyInfo keyInfo, InputContext context)
    {
        if (_shortcuts.TryGetValue((keyInfo.Key, keyInfo.Modifiers), out var action))
        {
            action(context);
            return true;
        }

        return base.HandleKey(keyInfo, context);
    }
}

/// <summary>
/// Handler that delegates to the focused widget.
/// </summary>
public class FocusedWidgetHandler : InputHandler
{
    public override bool HandleKey(ConsoleKeyInfo keyInfo, InputContext context)
    {
        if (context.FocusedWidget != null && context.FocusedWidget.HandleKey(keyInfo))
        {
            return true;
        }

        return base.HandleKey(keyInfo, context);
    }

    public override bool HandleMouse(MouseEvent mouseEvent, InputContext context)
    {
        // Hit test widgets
        foreach (var widget in context.Widgets.Reverse())
        {
            if (!widget.Visible)
                continue;

            if (widget.Bounds.Contains(mouseEvent.X, mouseEvent.Y))
            {
                // Check if widget has mouse event handlers
                var handled = InvokeWidgetMouseHandler(widget, mouseEvent);
                if (handled)
                    return true;
            }
        }

        return base.HandleMouse(mouseEvent, context);
    }

    private bool InvokeWidgetMouseHandler(Widget widget, MouseEvent mouseEvent)
    {
        // Use reflection to check for mouse event handlers
        // This is a placeholder - actual implementation would depend on widget events
        return false;
    }
}

/// <summary>
/// Handler for accessibility features (keyboard-only navigation).
/// </summary>
public class AccessibilityHandler : InputHandler
{
    public override bool HandleKey(ConsoleKeyInfo keyInfo, InputContext context)
    {
        // Handle special accessibility keys
        // For example, announce focused widget with screen reader
        return base.HandleKey(keyInfo, context);
    }
}
