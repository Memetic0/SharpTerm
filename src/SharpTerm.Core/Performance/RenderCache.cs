using System.Text;

namespace SharpTerm.Core.Performance;

/// <summary>
/// Represents a cached render output for a widget.
/// </summary>
public class CachedRender
{
    public string Output { get; set; } = string.Empty;
    public Rectangle Bounds { get; set; }
    public int WidgetHashCode { get; set; }

    public bool IsValid(Widget widget)
    {
        return WidgetHashCode == ComputeWidgetHash(widget)
            && Bounds.Equals(widget.Bounds);
    }

    public static int ComputeWidgetHash(Widget widget)
    {
        var hash = new HashCode();
        hash.Add(widget.GetType());
        hash.Add(widget.Bounds);
        hash.Add(widget.Visible);
        hash.Add(widget.ForegroundColor);
        hash.Add(widget.BackgroundColor);

        // Add widget-specific properties
        switch (widget)
        {
            case Widgets.Label label:
                hash.Add(label.Text);
                hash.Add(label.Alignment);
                break;
            case Widgets.Border border:
                hash.Add(border.Title);
                hash.Add(border.Subtitle);
                hash.Add(border.Style);
                hash.Add(border.ShowShadow);
                break;
            case Widgets.ProgressBar progressBar:
                hash.Add(progressBar.Value);
                hash.Add(progressBar.Maximum);
                hash.Add(progressBar.Style);
                hash.Add(progressBar.ShowPercentage);
                break;
        }

        return hash.ToHashCode();
    }
}

/// <summary>
/// Cache for rendered widget output to avoid re-rendering static content.
/// </summary>
public class RenderCache
{
    private readonly Dictionary<Widget, CachedRender> _cache = new();
    private readonly int _maxCacheSize;

    public RenderCache(int maxCacheSize = 100)
    {
        _maxCacheSize = maxCacheSize;
    }

    /// <summary>
    /// Gets cached render output if available and valid.
    /// </summary>
    public bool TryGetCached(Widget widget, out string? output)
    {
        output = null;

        if (!_cache.TryGetValue(widget, out var cached))
            return false;

        if (!cached.IsValid(widget))
        {
            _cache.Remove(widget);
            return false;
        }

        output = cached.Output;
        return true;
    }

    /// <summary>
    /// Caches the render output for a widget.
    /// </summary>
    public void Cache(Widget widget, string output)
    {
        // Limit cache size
        if (_cache.Count >= _maxCacheSize && !_cache.ContainsKey(widget))
        {
            // Remove oldest entry (simple FIFO)
            var firstKey = _cache.Keys.First();
            _cache.Remove(firstKey);
        }

        _cache[widget] = new CachedRender
        {
            Output = output,
            Bounds = widget.Bounds,
            WidgetHashCode = CachedRender.ComputeWidgetHash(widget)
        };
    }

    /// <summary>
    /// Invalidates the cache for a specific widget.
    /// </summary>
    public void Invalidate(Widget widget)
    {
        _cache.Remove(widget);
    }

    /// <summary>
    /// Clears the entire cache.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    public (int Count, int MaxSize) GetStats()
    {
        return (_cache.Count, _maxCacheSize);
    }
}

/// <summary>
/// Extension methods for using render cache with terminal driver.
/// </summary>
public static class RenderCacheExtensions
{
    /// <summary>
    /// Renders a widget using the cache if possible.
    /// </summary>
    public static void RenderCached(this ITerminalDriver driver, Widget widget, RenderCache cache)
    {
        if (cache.TryGetCached(widget, out var cachedOutput) && cachedOutput != null)
        {
            // Use cached output
            var sb = new StringBuilder();
            var tempDriver = new CachingTerminalDriver(sb);

            // Simply write the cached output
            Console.Write(cachedOutput);
        }
        else
        {
            // Render normally and cache
            var sb = new StringBuilder();
            var cachingDriver = new CachingTerminalDriver(sb);

            widget.Render(cachingDriver);
            cachingDriver.Flush();

            var output = sb.ToString();
            cache.Cache(widget, output);

            // Write to actual terminal
            Console.Write(output);
        }
    }
}

/// <summary>
/// A terminal driver that captures output to a string for caching.
/// </summary>
internal class CachingTerminalDriver : ITerminalDriver
{
    private readonly StringBuilder _output;

    public CachingTerminalDriver(StringBuilder output)
    {
        _output = output;
    }

    public int Width => Console.WindowWidth;
    public int Height => Console.WindowHeight;
    public bool KeyAvailable => false;
    public bool HasInputEvents => false;

    public void SetCursorPosition(int x, int y)
    {
        _output.Append(AnsiCache.GetCursorPosition(x, y));
    }

    public void Write(string text, Color foreground, Color background)
    {
        _output.Append(AnsiCache.GetForegroundColor(foreground));

        if (background.R != 0 || background.G != 0 || background.B != 1)
        {
            _output.Append(AnsiCache.GetBackgroundColor(background));
        }

        _output.Append(text);
        _output.Append("\x1b[0m");
    }

    public void Flush()
    {
        // Output is already in the StringBuilder
    }

    public void Clear()
    {
        _output.Append("\x1b[H\x1b[J");
    }

    public ConsoleKeyInfo ReadKey(bool intercept = true)
    {
        throw new NotSupportedException("Reading input not supported in caching driver");
    }

    public ConsoleEventType ReadConsoleEvent(out ConsoleKeyInfo? keyInfo, out MouseEvent? mouseEvent)
    {
        keyInfo = null;
        mouseEvent = null;
        return ConsoleEventType.None;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
