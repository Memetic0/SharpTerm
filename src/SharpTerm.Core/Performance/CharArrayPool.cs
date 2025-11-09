using System.Buffers;

namespace SharpTerm.Core.Performance;

/// <summary>
/// Pool for char arrays to reduce allocations during rendering.
/// </summary>
public static class CharArrayPool
{
    private static readonly ArrayPool<char> _pool = ArrayPool<char>.Shared;

    /// <summary>
    /// Rents a char array from the pool.
    /// </summary>
    public static char[] Rent(int minimumLength)
    {
        return _pool.Rent(minimumLength);
    }

    /// <summary>
    /// Returns a char array to the pool.
    /// </summary>
    public static void Return(char[] array, bool clearArray = false)
    {
        if (array != null)
        {
            _pool.Return(array, clearArray);
        }
    }

    /// <summary>
    /// Executes an action with a rented char array and returns it to the pool.
    /// </summary>
    public static void UseArray(int minimumLength, Action<char[], int> action)
    {
        var array = Rent(minimumLength);
        try
        {
            action(array, minimumLength);
        }
        finally
        {
            Return(array);
        }
    }

    /// <summary>
    /// Executes a function with a rented char array and returns it to the pool.
    /// </summary>
    public static T UseArray<T>(int minimumLength, Func<char[], int, T> func)
    {
        var array = Rent(minimumLength);
        try
        {
            return func(array, minimumLength);
        }
        finally
        {
            Return(array);
        }
    }
}
