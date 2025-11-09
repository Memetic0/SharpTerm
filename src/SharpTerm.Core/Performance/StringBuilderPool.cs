using System.Text;

namespace SharpTerm.Core.Performance;

/// <summary>
/// Pool for StringBuilder instances to reduce allocations.
/// </summary>
public static class StringBuilderPool
{
    [ThreadStatic]
    private static StringBuilder? _cachedInstance;

    private const int MaxBuilderSize = 8192; // 8KB
    private const int DefaultCapacity = 4096; // 4KB

    /// <summary>
    /// Gets a pooled StringBuilder instance.
    /// </summary>
    public static StringBuilder Acquire(int capacity = DefaultCapacity)
    {
        var sb = _cachedInstance;
        if (sb != null)
        {
            _cachedInstance = null;

            // Ensure capacity is adequate
            if (sb.Capacity < capacity)
            {
                sb.Capacity = capacity;
            }

            return sb;
        }

        return new StringBuilder(capacity);
    }

    /// <summary>
    /// Returns a StringBuilder to the pool.
    /// </summary>
    public static void Release(StringBuilder sb)
    {
        if (sb == null)
            return;

        // Don't pool very large builders
        if (sb.Capacity > MaxBuilderSize)
            return;

        sb.Clear();

        // Return to pool
        _cachedInstance = sb;
    }

    /// <summary>
    /// Gets a pooled StringBuilder, executes an action, and returns it to the pool.
    /// </summary>
    public static string GetString(Action<StringBuilder> action, int capacity = DefaultCapacity)
    {
        var sb = Acquire(capacity);
        try
        {
            action(sb);
            return sb.ToString();
        }
        finally
        {
            Release(sb);
        }
    }
}
