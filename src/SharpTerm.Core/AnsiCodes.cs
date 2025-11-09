namespace SharpTerm.Core;

/// <summary>
/// ANSI escape code constants and helpers (ECMA-48 standard).
/// </summary>
public static class AnsiCodes
{
    public const string ESC = "\x1b";
    public const string CSI = ESC + "["; // Control Sequence Introducer

    // Cursor Movement
    public static string MoveTo(int row, int col) => $"{CSI}{row + 1};{col + 1}H";

    public static string MoveUp(int n = 1) => $"{CSI}{n}A";

    public static string MoveDown(int n = 1) => $"{CSI}{n}B";

    public static string MoveRight(int n = 1) => $"{CSI}{n}C";

    public static string MoveLeft(int n = 1) => $"{CSI}{n}D";

    public static string MoveToColumn(int col) => $"{CSI}{col + 1}G";

    // Screen Control
    public static string ClearScreen => $"{CSI}2J";
    public static string ClearToEnd => $"{CSI}J";
    public static string ClearToBeginning => $"{CSI}1J";
    public static string ClearLine => $"{CSI}2K";
    public static string ClearToLineEnd => $"{CSI}K";
    public static string ClearToLineBeginning => $"{CSI}1K";
    public static string SaveCursor => $"{CSI}s";
    public static string RestoreCursor => $"{CSI}u";
    public static string HideCursor => $"{CSI}?25l";
    public static string ShowCursor => $"{CSI}?25h";

    // Alternate Screen
    public static string EnterAltScreen => $"{CSI}?1049h";
    public static string ExitAltScreen => $"{CSI}?1049l";

    // Colors (24-bit RGB)
    public static string Rgb(byte r, byte g, byte b) => $"{CSI}38;2;{r};{g};{b}m";

    public static string BgRgb(byte r, byte g, byte b) => $"{CSI}48;2;{r};{g};{b}m";

    public static string Rgb(Color color) => Rgb(color.R, color.G, color.B);

    public static string BgRgb(Color color) => BgRgb(color.R, color.G, color.B);

    // 256 Color Support
    public static string Color256(byte colorIndex) => $"{CSI}38;5;{colorIndex}m";

    public static string BgColor256(byte colorIndex) => $"{CSI}48;5;{colorIndex}m";

    // Text Styling
    public static string Bold => $"{CSI}1m";
    public static string Dim => $"{CSI}2m";
    public static string Italic => $"{CSI}3m";
    public static string Underline => $"{CSI}4m";
    public static string Blink => $"{CSI}5m";
    public static string Reverse => $"{CSI}7m";
    public static string Hidden => $"{CSI}8m";
    public static string Strikethrough => $"{CSI}9m";
    public static string Reset => $"{CSI}0m";

    // Style Reset
    public static string ResetBold => $"{CSI}22m";
    public static string ResetDim => $"{CSI}22m";
    public static string ResetItalic => $"{CSI}23m";
    public static string ResetUnderline => $"{CSI}24m";
    public static string ResetBlink => $"{CSI}25m";
    public static string ResetReverse => $"{CSI}27m";

    // Mouse Support
    public static string EnableMouseTracking => $"{CSI}?1000h{CSI}?1003h{CSI}?1006h";
    public static string DisableMouseTracking => $"{CSI}?1000l{CSI}?1003l{CSI}?1006l";
}
