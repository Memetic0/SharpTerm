namespace SharpTerm.Core.Performance;

/// <summary>
/// Caches terminal dimensions to avoid expensive Console property accesses.
/// </summary>
public class TerminalDimensionCache
{
    private int _cachedWidth;
    private int _cachedHeight;
    private DateTime _lastUpdate;
    private readonly TimeSpan _cacheLifetime;

    public TerminalDimensionCache(TimeSpan? cacheLifetime = null)
    {
        _cacheLifetime = cacheLifetime ?? TimeSpan.FromMilliseconds(100);
        UpdateCache();
    }

    /// <summary>
    /// Gets the cached terminal width.
    /// </summary>
    public int Width
    {
        get
        {
            CheckAndUpdate();
            return _cachedWidth;
        }
    }

    /// <summary>
    /// Gets the cached terminal height.
    /// </summary>
    public int Height
    {
        get
        {
            CheckAndUpdate();
            return _cachedHeight;
        }
    }

    /// <summary>
    /// Gets both width and height in a single property access.
    /// </summary>
    public (int width, int height) Dimensions
    {
        get
        {
            CheckAndUpdate();
            return (_cachedWidth, _cachedHeight);
        }
    }

    /// <summary>
    /// Forces an immediate cache update.
    /// </summary>
    public void ForceUpdate()
    {
        UpdateCache();
    }

    /// <summary>
    /// Checks if dimensions have changed since last update.
    /// </summary>
    public bool HasChanged()
    {
        try
        {
            var currentWidth = Console.WindowWidth;
            var currentHeight = Console.WindowHeight;

            return currentWidth != _cachedWidth || currentHeight != _cachedHeight;
        }
        catch
        {
            return false;
        }
    }

    private void CheckAndUpdate()
    {
        var now = DateTime.UtcNow;
        if (now - _lastUpdate > _cacheLifetime)
        {
            UpdateCache();
        }
    }

    private void UpdateCache()
    {
        try
        {
            _cachedWidth = Console.WindowWidth;
            _cachedHeight = Console.WindowHeight;
            _lastUpdate = DateTime.UtcNow;
        }
        catch
        {
            // If we can't read dimensions, keep previous values
        }
    }

    /// <summary>
    /// Gets the total area of the terminal.
    /// </summary>
    public int Area => Width * Height;

    /// <summary>
    /// Creates a rectangle representing the entire terminal.
    /// </summary>
    public Rectangle GetBounds()
    {
        return new Rectangle(0, 0, Width, Height);
    }
}

/// <summary>
/// Global terminal dimension cache singleton.
/// </summary>
public static class TerminalDimensions
{
    private static readonly TerminalDimensionCache _cache = new();

    /// <summary>
    /// Gets the current terminal width (cached).
    /// </summary>
    public static int Width => _cache.Width;

    /// <summary>
    /// Gets the current terminal height (cached).
    /// </summary>
    public static int Height => _cache.Height;

    /// <summary>
    /// Gets both dimensions (cached).
    /// </summary>
    public static (int width, int height) GetDimensions() => _cache.Dimensions;

    /// <summary>
    /// Forces a cache update.
    /// </summary>
    public static void Refresh() => _cache.ForceUpdate();

    /// <summary>
    /// Checks if terminal was resized.
    /// </summary>
    public static bool HasResized() => _cache.HasChanged();

    /// <summary>
    /// Gets the terminal bounds.
    /// </summary>
    public static Rectangle GetBounds() => _cache.GetBounds();
}
