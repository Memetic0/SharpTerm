using System.Runtime.InteropServices;

namespace SharpTerm.Core.Platform;

/// <summary>
/// Detects and reports terminal capabilities.
/// </summary>
public class TerminalCapabilities
{
    private static TerminalCapabilities? _current;

    /// <summary>
    /// Gets the capabilities for the current terminal.
    /// </summary>
    public static TerminalCapabilities Current => _current ??= Detect();

    public bool SupportsTrueColor { get; init; }
    public bool Supports256Colors { get; init; }
    public bool SupportsMouseInput { get; init; }
    public bool SupportsAlternateScreen { get; init; }
    public bool SupportsUnicode { get; init; }
    public bool SupportsCursorShapes { get; init; }
    public bool SupportsHyperlinks { get; init; }
    public bool SupportsSixelGraphics { get; init; }
    public bool SupportsKittyGraphics { get; init; }
    public bool SupportsITerm2Images { get; init; }
    public bool SupportsBracketedPaste { get; init; }
    public int MaxColors { get; init; }
    public string TerminalType { get; init; } = "unknown";
    public string ColorTerm { get; init; } = "";

    /// <summary>
    /// Detects terminal capabilities.
    /// </summary>
    public static TerminalCapabilities Detect()
    {
        var termType = Environment.GetEnvironmentVariable("TERM") ?? "unknown";
        var colorTerm = Environment.GetEnvironmentVariable("COLORTERM") ?? "";
        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM") ?? "";

        // Detect true color support
        var supportsTrueColor =
            colorTerm.Contains("truecolor") ||
            colorTerm.Contains("24bit") ||
            termProgram == "iTerm.app" ||
            termProgram == "WezTerm" ||
            termType.Contains("256color");

        // Detect 256 color support
        var supports256Colors =
            supportsTrueColor ||
            termType.Contains("256color") ||
            termType.Contains("256");

        // Detect mouse support
        var supportsMouseInput =
            termType.Contains("xterm") ||
            termType.Contains("screen") ||
            termType.Contains("tmux") ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // Detect alternate screen buffer support
        var supportsAlternateScreen =
            termType.Contains("xterm") ||
            termType.Contains("screen") ||
            termType.Contains("tmux") ||
            termType.Contains("rxvt") ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // Detect Unicode support
        var supportsUnicode =
            (Environment.GetEnvironmentVariable("LANG")?.Contains("UTF-8") ?? false) ||
            (Environment.GetEnvironmentVariable("LC_ALL")?.Contains("UTF-8") ?? false) ||
            Console.OutputEncoding.WebName.Contains("utf", StringComparison.OrdinalIgnoreCase);

        // Detect cursor shapes
        var supportsCursorShapes =
            termType.Contains("xterm") ||
            termType.Contains("screen") ||
            termProgram == "iTerm.app";

        // Detect hyperlinks (OSC 8)
        var supportsHyperlinks =
            termProgram == "iTerm.app" ||
            termProgram == "WezTerm" ||
            termProgram == "vscode" ||
            termType.Contains("kitty");

        // Detect graphics protocols
        var supportsSixel = termType.Contains("xterm") && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var supportsKitty = termType.Contains("kitty");
        var supportsITerm2 = termProgram == "iTerm.app";

        // Detect bracketed paste
        var supportsBracketedPaste =
            termType.Contains("xterm") ||
            termType.Contains("screen") ||
            termType.Contains("tmux");

        var maxColors = supportsTrueColor ? 16777216 : (supports256Colors ? 256 : 16);

        return new TerminalCapabilities
        {
            SupportsTrueColor = supportsTrueColor,
            Supports256Colors = supports256Colors,
            SupportsMouseInput = supportsMouseInput,
            SupportsAlternateScreen = supportsAlternateScreen,
            SupportsUnicode = supportsUnicode,
            SupportsCursorShapes = supportsCursorShapes,
            SupportsHyperlinks = supportsHyperlinks,
            SupportsSixelGraphics = supportsSixel,
            SupportsKittyGraphics = supportsKitty,
            SupportsITerm2Images = supportsITerm2,
            SupportsBracketedPaste = supportsBracketedPaste,
            MaxColors = maxColors,
            TerminalType = termType,
            ColorTerm = colorTerm
        };
    }

    /// <summary>
    /// Gets the best supported color mode.
    /// </summary>
    public ColorMode GetBestColorMode()
    {
        if (SupportsTrueColor) return ColorMode.TrueColor;
        if (Supports256Colors) return ColorMode.Colors256;
        return ColorMode.Colors16;
    }

    /// <summary>
    /// Creates a degraded color from RGB based on terminal capabilities.
    /// </summary>
    public Color DegradeColor(Color color)
    {
        if (SupportsTrueColor)
            return color;

        if (Supports256Colors)
            return DegradeTo256Color(color);

        return DegradeTo16Color(color);
    }

    private Color DegradeTo256Color(Color color)
    {
        // Convert RGB to closest 256-color palette color
        // This is a simplified approximation
        if (color.R == color.G && color.G == color.B)
        {
            // Grayscale
            var gray = (byte)((color.R + color.G + color.B) / 3);
            return new Color(gray, gray, gray);
        }

        // Snap to 6x6x6 color cube
        var r = (byte)((color.R * 5 / 255) * 51);
        var g = (byte)((color.G * 5 / 255) * 51);
        var b = (byte)((color.B * 5 / 255) * 51);
        return new Color(r, g, b);
    }

    private Color DegradeTo16Color(Color color)
    {
        // Map to nearest basic 16 color
        var brightness = (color.R + color.G + color.B) / 3;
        var isBright = brightness > 128;

        if (color.R > color.G && color.R > color.B)
            return isBright ? Color.Red : new Color(128, 0, 0);
        if (color.G > color.R && color.G > color.B)
            return isBright ? Color.Green : new Color(0, 128, 0);
        if (color.B > color.R && color.B > color.G)
            return isBright ? Color.Blue : new Color(0, 0, 128);

        return isBright ? Color.White : Color.Black;
    }
}

/// <summary>
/// Supported color modes.
/// </summary>
public enum ColorMode
{
    /// <summary>
    /// 16 basic colors (ANSI).
    /// </summary>
    Colors16,

    /// <summary>
    /// 256 color palette.
    /// </summary>
    Colors256,

    /// <summary>
    /// 24-bit RGB true color.
    /// </summary>
    TrueColor
}
