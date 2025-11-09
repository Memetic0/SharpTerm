namespace SharpTerm.Core;

/// <summary>
/// Interface for terminal/console interaction abstraction.
/// </summary>
public interface ITerminalDriver : IDisposable
{
    /// <summary>
    /// Gets the current width of the terminal in columns.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the current height of the terminal in rows.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Sets the cursor position (buffered until Flush is called).
    /// </summary>
    void SetCursorPosition(int x, int y);

    /// <summary>
    /// Writes text with specified colors (buffered until Flush is called).
    /// </summary>
    void Write(string text, Color foreground, Color background);

    /// <summary>
    /// Flushes all buffered output to the terminal.
    /// </summary>
    void Flush();

    /// <summary>
    /// Clears the terminal screen.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets whether there are any input events available (keyboard or mouse).
    /// </summary>
    bool HasInputEvents { get; }

    /// <summary>
    /// Gets whether a key press is available to read.
    /// </summary>
    bool KeyAvailable { get; }

    /// <summary>
    /// Reads a key from the console.
    /// </summary>
    ConsoleKeyInfo ReadKey(bool intercept = true);

    /// <summary>
    /// Tries to read any console event. Returns the event type and outputs the specific event data.
    /// </summary>
    ConsoleEventType ReadConsoleEvent(out ConsoleKeyInfo? keyInfo, out MouseEvent? mouseEvent);
}

/// <summary>
/// Represents a mouse event.
/// </summary>
public record MouseEvent(int X, int Y, bool IsLeftClick, int ScrollDelta = 0);

/// <summary>
/// Type of console event.
/// </summary>
public enum ConsoleEventType
{
    None,
    Keyboard,
    Mouse,
    Other,
}
