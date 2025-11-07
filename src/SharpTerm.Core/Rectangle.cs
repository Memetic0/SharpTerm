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

    public override string ToString() => $"({X}, {Y}, {Width}x{Height})";
}
