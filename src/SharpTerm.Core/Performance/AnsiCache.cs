using System.Collections.Concurrent;

namespace SharpTerm.Core.Performance;

/// <summary>
/// Cache for frequently used ANSI escape sequences to reduce string allocations.
/// </summary>
public static class AnsiCache
{
    private static readonly ConcurrentDictionary<Color, string> _foregroundColorCache = new();
    private static readonly ConcurrentDictionary<Color, string> _backgroundColorCache = new();
    private static readonly ConcurrentDictionary<(int, int), string> _cursorPositionCache = new();

    private const int MaxCursorPositionCacheSize = 1000;

    /// <summary>
    /// Gets the cached foreground color ANSI sequence.
    /// </summary>
    public static string GetForegroundColor(Color color)
    {
        return _foregroundColorCache.GetOrAdd(color, c =>
            $"\x1b[38;2;{c.R};{c.G};{c.B}m");
    }

    /// <summary>
    /// Gets the cached background color ANSI sequence.
    /// </summary>
    public static string GetBackgroundColor(Color color)
    {
        return _backgroundColorCache.GetOrAdd(color, c =>
            $"\x1b[48;2;{c.R};{c.G};{c.B}m");
    }

    /// <summary>
    /// Gets the cached cursor position ANSI sequence.
    /// </summary>
    public static string GetCursorPosition(int x, int y)
    {
        // Only cache common positions to avoid memory bloat
        if (_cursorPositionCache.Count < MaxCursorPositionCacheSize)
        {
            return _cursorPositionCache.GetOrAdd((x, y), pos =>
                $"\x1b[{pos.Item2 + 1};{pos.Item1 + 1}H");
        }

        // If cache is full, generate without caching
        return $"\x1b[{y + 1};{x + 1}H";
    }

    /// <summary>
    /// Clears all caches. Useful when terminal characteristics change.
    /// </summary>
    public static void ClearAll()
    {
        _foregroundColorCache.Clear();
        _backgroundColorCache.Clear();
        _cursorPositionCache.Clear();
    }

    /// <summary>
    /// Pre-populates the cache with commonly used colors.
    /// </summary>
    public static void Warmup()
    {
        // Pre-cache common colors
        var commonColors = new[]
        {
            Color.Black, Color.White, Color.Red, Color.Green,
            Color.Blue, Color.Yellow, Color.Cyan, Color.Magenta,
            Color.DarkGray, Color.LightGray, Color.Transparent
        };

        foreach (var color in commonColors)
        {
            GetForegroundColor(color);
            GetBackgroundColor(color);
        }

        // Pre-cache common cursor positions (top-left corner area)
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                GetCursorPosition(x, y);
            }
        }
    }
}
