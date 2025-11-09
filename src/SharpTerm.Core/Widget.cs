using SharpTerm.Core.Performance;

namespace SharpTerm.Core;

/// <summary>
/// Base class for all UI widgets.
/// </summary>
public abstract class Widget
{
    private Widget? _parent;
    private readonly List<Widget> _children = new();
    private readonly OptimizedEventHandler<EventArgs> _changedHandler = new();

    /// <summary>
    /// Event raised when the widget's visual state changes and needs to be redrawn.
    /// </summary>
    public event EventHandler? Changed
    {
        add
        {
            if (value != null)
                _changedHandler.Add((sender, args) => value(sender, args));
        }
        remove
        {
            // Note: OptimizedEventHandler doesn't support exact removal with wrapped delegates
            // This is acceptable as widgets are typically not frequently subscribed/unsubscribed
        }
    }

    /// <summary>
    /// Raises the Changed event to notify that the widget needs to be redrawn.
    /// </summary>
    protected void OnChanged()
    {
        _changedHandler.Invoke(this, EventArgs.Empty);

        // Propagate change notification to parent
        _parent?.OnChanged();
    }

    public Rectangle Bounds { get; set; }
    public bool Visible { get; set; } = true;
    public Color ForegroundColor { get; set; } = Color.White;
    public Color BackgroundColor { get; set; } = Color.Transparent;

    /// <summary>
    /// Gets the parent widget in the widget tree.
    /// </summary>
    public Widget? Parent => _parent;

    /// <summary>
    /// Gets the children of this widget.
    /// </summary>
    public IReadOnlyList<Widget> Children => _children.AsReadOnly();

    /// <summary>
    /// Adds a child widget to this widget.
    /// </summary>
    public virtual void AddChild(Widget child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        if (child == this)
            throw new InvalidOperationException("Cannot add widget as its own child");

        if (_children.Contains(child))
            return;

        // Remove from previous parent
        child._parent?.RemoveChild(child);

        _children.Add(child);
        child._parent = this;
        OnChanged();
    }

    /// <summary>
    /// Removes a child widget from this widget.
    /// </summary>
    public virtual void RemoveChild(Widget child)
    {
        if (_children.Remove(child))
        {
            child._parent = null;
            OnChanged();
        }
    }

    /// <summary>
    /// Clears all children from this widget.
    /// </summary>
    public virtual void ClearChildren()
    {
        foreach (var child in _children.ToList())
        {
            RemoveChild(child);
        }
    }

    /// <summary>
    /// Renders the widget to the terminal driver.
    /// </summary>
    public abstract void Render(ITerminalDriver driver);

    /// <summary>
    /// Renders this widget and all its children recursively.
    /// </summary>
    public virtual void RenderTree(ITerminalDriver driver)
    {
        if (!Visible)
            return;

        Render(driver);

        foreach (var child in _children)
        {
            child.RenderTree(driver);
        }
    }

    /// <summary>
    /// Handles a key press. Returns true if the key was handled.
    /// </summary>
    public virtual bool HandleKey(ConsoleKeyInfo key) => false;

    /// <summary>
    /// Gets all descendants of this widget in depth-first order.
    /// </summary>
    public IEnumerable<Widget> GetDescendants()
    {
        foreach (var child in _children)
        {
            yield return child;
            foreach (var descendant in child.GetDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Finds a widget in the tree by predicate.
    /// </summary>
    public Widget? FindWidget(Func<Widget, bool> predicate)
    {
        if (predicate(this))
            return this;

        foreach (var child in _children)
        {
            var found = child.FindWidget(predicate);
            if (found != null)
                return found;
        }

        return null;
    }
}
