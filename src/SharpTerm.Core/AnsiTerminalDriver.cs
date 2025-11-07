using System.Runtime.InteropServices;
using System.Text;

namespace SharpTerm.Core;

/// <summary>
/// Cross-platform terminal driver using ANSI escape sequences.
/// </summary>
public class AnsiTerminalDriver : ITerminalDriver
{
    private readonly StringBuilder _buffer = new(4096);
    private bool _isInitialized;
    private IntPtr _consoleInputHandle;
    private uint _previousConsoleMode;
    
    public int Width => Console.WindowWidth;
    public int Height => Console.WindowHeight;
    
    public AnsiTerminalDriver()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        if (_isInitialized) return;
        
        // Enable virtual terminal processing on Windows
        if (OperatingSystem.IsWindows())
        {
            EnableWindowsVirtualTerminal();
            EnableWindowsMouseInput();
        }
        
        // Set console encoding to UTF-8 for box-drawing characters
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        
        // Hide cursor and use alternate screen buffer
        Console.CursorVisible = false;
        _buffer.Append("\x1b[?1049h"); // Enter alternate screen
        _buffer.Append("\x1b[?25l");   // Hide cursor
        Flush();
        
        _isInitialized = true;
    }
    
    private static void EnableWindowsVirtualTerminal()
    {
        try
        {
            var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
            if (GetConsoleMode(handle, out uint mode))
            {
                mode |= 0x0004; // ENABLE_VIRTUAL_TERMINAL_PROCESSING
                SetConsoleMode(handle, mode);
            }
        }
        catch
        {
            // Ignore errors - terminal may not support VT processing
        }
    }
    
    private void EnableWindowsMouseInput()
    {
        try
        {
            _consoleInputHandle = GetStdHandle(-10); // STD_INPUT_HANDLE
            if (GetConsoleMode(_consoleInputHandle, out _previousConsoleMode))
            {
                uint mode = _previousConsoleMode;
                mode |= 0x0010;  // ENABLE_MOUSE_INPUT
                mode &= ~0x0040u; // DISABLE_QUICK_EDIT_MODE
                mode |= 0x0080;  // ENABLE_EXTENDED_FLAGS
                SetConsoleMode(_consoleInputHandle, mode);
            }
        }
        catch
        {
            // Ignore errors
        }
    }
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    
    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
    
    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetNumberOfConsoleInputEvents(IntPtr hConsoleInput, out uint lpcNumberOfEvents);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);
    
    [StructLayout(LayoutKind.Explicit)]
    private struct INPUT_RECORD
    {
        [FieldOffset(0)]
        public ushort EventType;
        [FieldOffset(4)]
        public KEY_EVENT_RECORD KeyEvent;
        [FieldOffset(4)]
        public MOUSE_EVENT_RECORD MouseEvent;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct KEY_EVENT_RECORD
    {
        public int bKeyDown;          // BOOL is 4 bytes in Windows API
        public ushort wRepeatCount;
        public ushort wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public ushort UnicodeChar;    // WCHAR is 2 bytes
        public uint dwControlKeyState;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSE_EVENT_RECORD
    {
        public COORD dwMousePosition;
        public uint dwButtonState;
        public uint dwControlKeyState;
        public uint dwEventFlags;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct COORD
    {
        public short X;
        public short Y;
    }
    
    public void SetCursorPosition(int x, int y)
    {
        // Use ANSI escape codes instead of Console.SetCursorPosition for better performance
        // ANSI uses 1-based indexing
        _buffer.Append($"\x1b[{y + 1};{x + 1}H");
    }
    
    public void Write(string text, Color foreground, Color background)
    {
        // Batch ANSI codes for efficiency
        _buffer.Append($"\x1b[38;2;{foreground.R};{foreground.G};{foreground.B}m");
        
        // Only set background if not transparent (R=0, G=0, B=1)
        if (background.R != 0 || background.G != 0 || background.B != 1)
        {
            _buffer.Append($"\x1b[48;2;{background.R};{background.G};{background.B}m");
        }
        
        _buffer.Append(text);
        _buffer.Append("\x1b[0m"); // Reset
    }
    
    public void Flush()
    {
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
        _buffer.Append("\x1b[H\x1b[J");
    }
    
    public bool KeyAvailable => Console.KeyAvailable;
    
    public bool HasInputEvents
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    if (_consoleInputHandle != IntPtr.Zero &&
                        GetNumberOfConsoleInputEvents(_consoleInputHandle, out uint numEvents))
                    {
                        return numEvents > 0;
                    }
                }
                catch
                {
                    // Fall back to KeyAvailable
                }
            }
            return Console.KeyAvailable;
        }
    }
    
    public ConsoleKeyInfo ReadKey(bool intercept = true)
    {
        return Console.ReadKey(intercept);
    }
    
    public ConsoleEventType ReadConsoleEvent(out ConsoleKeyInfo? keyInfo, out MouseEvent? mouseEvent)
    {
        keyInfo = null;
        mouseEvent = null;
        
        if (!OperatingSystem.IsWindows())
        {
            if (Console.KeyAvailable)
            {
                keyInfo = Console.ReadKey(true);
                return ConsoleEventType.Keyboard;
            }
            return ConsoleEventType.None;
        }
        
        // On Windows with mouse input enabled, read all events via ReadConsoleInput
        try
        {
            if (GetNumberOfConsoleInputEvents(_consoleInputHandle, out uint numEvents) && numEvents > 0)
            {
                var buffer = new INPUT_RECORD[1];
                if (ReadConsoleInput(_consoleInputHandle, buffer, 1, out uint eventsRead) && eventsRead > 0)
                {
                    var record = buffer[0];
                    
                    if (record.EventType == 1) // KEY_EVENT
                    {
                        var keyEvent = record.KeyEvent;
                        // Only process key down events (bKeyDown != 0 means key down)
                        if (keyEvent.bKeyDown != 0)
                        {
                            char keyChar = (char)keyEvent.UnicodeChar;
                            ConsoleKey key = (ConsoleKey)keyEvent.wVirtualKeyCode;
                            bool shift = (keyEvent.dwControlKeyState & 0x0010) != 0;
                            bool alt = (keyEvent.dwControlKeyState & 0x0003) != 0;
                            bool control = (keyEvent.dwControlKeyState & 0x000C) != 0;
                            
                            keyInfo = new ConsoleKeyInfo(keyChar, key, shift, alt, control);
                            return ConsoleEventType.Keyboard;
                        }
                        // Key up event - consumed
                        return ConsoleEventType.Other;
                    }
                    else if (record.EventType == 2) // MOUSE_EVENT
                    {
                        var mouseRecord = record.MouseEvent;
                        
                        // Check for scroll wheel (dwEventFlags = 0x0004)
                        if ((mouseRecord.dwEventFlags & 0x0004) != 0)
                        {
                            // Extract scroll direction from high word of dwButtonState
                            int scrollDelta = (short)((mouseRecord.dwButtonState >> 16) & 0xFFFF);
                            // Positive = scroll up, Negative = scroll down
                            // Normalize to +1 or -1
                            int normalizedDelta = scrollDelta > 0 ? 1 : -1;
                            
                            mouseEvent = new MouseEvent(
                                mouseRecord.dwMousePosition.X,
                                mouseRecord.dwMousePosition.Y,
                                false,
                                normalizedDelta
                            );
                            return ConsoleEventType.Mouse;
                        }
                        // Check for button clicks
                        else if (mouseRecord.dwEventFlags == 0 || mouseRecord.dwEventFlags == 1)
                        {
                            bool isLeftClick = (mouseRecord.dwButtonState & 0x0001) != 0;
                            if (isLeftClick)
                            {
                                mouseEvent = new MouseEvent(
                                    mouseRecord.dwMousePosition.X,
                                    mouseRecord.dwMousePosition.Y,
                                    true,
                                    0
                                );
                                return ConsoleEventType.Mouse;
                            }
                        }
                        // Non-click mouse event (move, etc.)
                        return ConsoleEventType.Other;
                    }
                    // Other event types (focus, resize, etc.)
                    return ConsoleEventType.Other;
                }
            }
        }
        catch
        {
            // Fall back
            if (Console.KeyAvailable)
            {
                keyInfo = Console.ReadKey(true);
                return ConsoleEventType.Keyboard;
            }
        }
        
        return ConsoleEventType.None;
    }
    
    public void Dispose()
    {
        // Restore console mode on Windows
        if (OperatingSystem.IsWindows() && _consoleInputHandle != IntPtr.Zero)
        {
            SetConsoleMode(_consoleInputHandle, _previousConsoleMode);
        }
        
        // Exit alternate screen buffer and show cursor
        _buffer.Append("\x1b[?25h");   // Show cursor
        _buffer.Append("\x1b[?1049l"); // Exit alternate screen
        Flush();
        Console.CursorVisible = true;
        Console.ResetColor();
        GC.SuppressFinalize(this);
    }
}
