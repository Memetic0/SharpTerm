namespace SharpTerm.Core;

/// <summary>
/// Represents an RGB color for terminal rendering.
/// </summary>
public readonly struct Color(byte r, byte g, byte b)
{
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;

    // Predefined colors
    public static Color Transparent => new(0, 0, 1); // Special value for transparent
    public static Color Black => new(0, 0, 0);
    public static Color White => new(255, 255, 255);
    public static Color Red => new(255, 0, 0);
    public static Color Green => new(0, 255, 0);
    public static Color Blue => new(0, 0, 255);
    public static Color Yellow => new(255, 255, 0);
    public static Color Cyan => new(0, 255, 255);
    public static Color Magenta => new(255, 0, 255);
    public static Color DarkGray => new(128, 128, 128);
    public static Color LightGray => new(192, 192, 192);

    public override string ToString() => $"RGB({R}, {G}, {B})";
}
