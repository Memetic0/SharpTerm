using System.Text;
using SharpTerm.Core.DriverLogic;
using SharpTerm.Core.Performance;

namespace SharpTerm.Core;

/// <summary>
/// Cross-platform terminal driver using ANSI escape sequences.
/// </summary>
public class AnsiTerminalDriver : ITerminalDriver
{
    private readonly StringBuilder _buffer = new(4096);
    private readonly AnsiCommandBatcher _batcher = new(4096);
    private readonly IPlatformProvider _platformProvider;
    private bool _isInitialized;
    private IntPtr _consoleInputHandle;
    private uint _previousConsoleMode;

    public int Width => Console.WindowWidth;
    public int Height => Console.WindowHeight;

    public AnsiTerminalDriver() : this(PlatformProvider.Create())
    {
    }

    public AnsiTerminalDriver(IPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider ?? throw new ArgumentNullException(nameof(platformProvider));
        Initialize();
    }

    private void Initialize()
    {
        if (_isInitialized)
            return;

        // Enable virtual terminal processing
        _platformProvider.EnableVirtualTerminal();

        // Enable mouse input
        var (handle, previousMode) = _platformProvider.EnableMouseInput();
        _consoleInputHandle = handle;
        _previousConsoleMode = previousMode;

        // Set console encoding to UTF-8 for box-drawing characters
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        // Hide cursor and use alternate screen buffer
        Console.CursorVisible = false;
        _buffer.Append("\x1b[?1049h"); // Enter alternate screen
        _buffer.Append("\x1b[?25l"); // Hide cursor
        Flush();

        _isInitialized = true;
    }

    public void SetCursorPosition(int x, int y)
    {
        // Clamp to terminal bounds to prevent errors on small resizes
        x = Math.Max(0, Math.Min(x, Width - 1));
        y = Math.Max(0, Math.Min(y, Height - 1));

        // Use AnsiCommandBatcher for stateful optimization (eliminates redundant cursor moves)
        _batcher.SetCursorPosition(x, y);
    }

    public void Write(string text, Color foreground, Color background)
    {
        // Use AnsiCommandBatcher for stateful optimization (eliminates redundant color changes)
        _batcher.SetForegroundColor(foreground);

        // Only set background if not transparent (R=0, G=0, B=1)
        if (background.R != 0 || background.G != 0 || background.B != 1)
        {
            _batcher.SetBackgroundColor(background);
        }

        _batcher.Write(text);
        _batcher.Reset(); // Reset after each write for safety
    }

    public void Flush()
    {
        // Flush AnsiCommandBatcher which handles auto-batching and state tracking
        _batcher.Flush();

        // Also flush old buffer for backward compatibility during transition
        if (_buffer.Length > 0)
        {
            Console.Write(_buffer.ToString());
            _buffer.Clear();
        }
    }

    public void Clear()
    {
        // Move cursor to home and clear from cursor to end of screen
        // This is faster and causes less flicker than \x1b[2J
        _batcher.Write("\x1b[H\x1b[J");
    }

    public bool KeyAvailable => Console.KeyAvailable;

    public bool HasInputEvents => _platformProvider.HasInputEvents(_consoleInputHandle);

    public ConsoleKeyInfo ReadKey(bool intercept = true)
    {
        return Console.ReadKey(intercept);
    }

    public ConsoleEventType ReadConsoleEvent(
        out ConsoleKeyInfo? keyInfo,
        out MouseEvent? mouseEvent
    )
    {
        return _platformProvider.ReadConsoleEvent(
            _consoleInputHandle,
            out keyInfo,
            out mouseEvent
        );
    }

    public void Dispose()
    {
        // Restore console mode
        if (_consoleInputHandle != IntPtr.Zero)
        {
            _platformProvider.RestoreConsoleMode(_consoleInputHandle, _previousConsoleMode);
        }

        // Exit alternate screen buffer and show cursor
        _buffer.Append("\x1b[?25h"); // Show cursor
        _buffer.Append("\x1b[?1049l"); // Exit alternate screen
        Flush();
        Console.CursorVisible = true;
        Console.ResetColor();
        GC.SuppressFinalize(this);
    }
}
