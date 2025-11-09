namespace SharpTerm.Core.Rendering;

/// <summary>
/// Renders a specific widget type to the terminal.
/// </summary>
public interface IWidgetRenderer
{
    /// <summary>
    /// Gets the type of widget this renderer can handle.
    /// </summary>
    Type WidgetType { get; }

    /// <summary>
    /// Renders the widget to the terminal driver.
    /// </summary>
    void Render(Widget widget, ITerminalDriver driver);

    /// <summary>
    /// Determines if this renderer can render the specified widget.
    /// </summary>
    bool CanRender(Widget widget);
}

/// <summary>
/// Generic base class for widget renderers.
/// </summary>
public abstract class WidgetRenderer<TWidget> : IWidgetRenderer where TWidget : Widget
{
    public Type WidgetType => typeof(TWidget);

    public void Render(Widget widget, ITerminalDriver driver)
    {
        if (widget is TWidget typedWidget)
        {
            RenderCore(typedWidget, driver);
        }
    }

    public bool CanRender(Widget widget)
    {
        return widget is TWidget;
    }

    /// <summary>
    /// Renders the typed widget.
    /// </summary>
    protected abstract void RenderCore(TWidget widget, ITerminalDriver driver);
}

/// <summary>
/// Manages widget renderers and delegates rendering to appropriate renderers.
/// </summary>
public class RendererRegistry
{
    private readonly Dictionary<Type, IWidgetRenderer> _renderers = new();
    private IWidgetRenderer? _defaultRenderer;

    /// <summary>
    /// Registers a renderer for a specific widget type.
    /// </summary>
    public void Register<TWidget>(IWidgetRenderer renderer) where TWidget : Widget
    {
        _renderers[typeof(TWidget)] = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    /// <summary>
    /// Registers a renderer.
    /// </summary>
    public void Register(IWidgetRenderer renderer)
    {
        if (renderer == null)
            throw new ArgumentNullException(nameof(renderer));

        _renderers[renderer.WidgetType] = renderer;
    }

    /// <summary>
    /// Sets the default renderer to use when no specific renderer is registered.
    /// </summary>
    public void SetDefaultRenderer(IWidgetRenderer renderer)
    {
        _defaultRenderer = renderer;
    }

    /// <summary>
    /// Gets the renderer for the specified widget type.
    /// </summary>
    public IWidgetRenderer? GetRenderer(Type widgetType)
    {
        if (_renderers.TryGetValue(widgetType, out var renderer))
            return renderer;

        // Check base types
        var currentType = widgetType.BaseType;
        while (currentType != null && currentType != typeof(object))
        {
            if (_renderers.TryGetValue(currentType, out renderer))
                return renderer;
            currentType = currentType.BaseType;
        }

        return _defaultRenderer;
    }

    /// <summary>
    /// Gets the renderer for the specified widget.
    /// </summary>
    public IWidgetRenderer? GetRenderer(Widget widget)
    {
        return GetRenderer(widget.GetType());
    }

    /// <summary>
    /// Renders a widget using the appropriate renderer.
    /// </summary>
    public void Render(Widget widget, ITerminalDriver driver)
    {
        var renderer = GetRenderer(widget);
        if (renderer != null && renderer.CanRender(widget))
        {
            renderer.Render(widget, driver);
        }
        else
        {
            // Fallback to widget's own Render method
            widget.Render(driver);
        }
    }
}

/// <summary>
/// Default renderer that delegates to widget's Render method.
/// </summary>
public class DefaultWidgetRenderer : IWidgetRenderer
{
    public Type WidgetType => typeof(Widget);

    public void Render(Widget widget, ITerminalDriver driver)
    {
        widget.Render(driver);
    }

    public bool CanRender(Widget widget)
    {
        return true;
    }
}
