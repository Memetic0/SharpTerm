namespace SharpTerm.Core;

/// <summary>
/// Represents a rectangular area in the terminal.
/// </summary>
public readonly struct Rectangle(int x, int y, int width, int height)
{
    public int X { get; } = x;
    public int Y { get; } = y;
    public int Width { get; } = width;
    public int Height { get; } = height;

    /// <summary>
    /// Determines if this rectangle contains the specified point.
    /// </summary>
    public bool Contains(int x, int y)
    {
        return x >= X && x < X + Width && y >= Y && y < Y + Height;
    }

    /// <summary>
    /// Determines if this rectangle intersects with another rectangle.
    /// </summary>
    public bool IntersectsWith(Rectangle other)
    {
        return X < other.X + other.Width &&
               X + Width > other.X &&
               Y < other.Y + other.Height &&
               Y + Height > other.Y;
    }

    public override string ToString() => $"({X}, {Y}, {Width}x{Height})";
}
