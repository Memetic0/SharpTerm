using System.Runtime.InteropServices;

namespace SharpTerm.Core.DriverLogic;

/// <summary>
/// Windows-specific functionality for terminal driver.
/// </summary>
internal static class WindowsConsole
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool GetNumberOfConsoleInputEvents(
        IntPtr hConsoleInput,
        out uint lpcNumberOfEvents
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool ReadConsoleInput(
        IntPtr hConsoleInput,
        [Out] INPUT_RECORD[] lpBuffer,
        uint nLength,
        out uint lpNumberOfEventsRead
    );

    [StructLayout(LayoutKind.Explicit)]
    internal struct INPUT_RECORD
    {
        [FieldOffset(0)]
        public ushort EventType;

        [FieldOffset(4)]
        public KEY_EVENT_RECORD KeyEvent;

        [FieldOffset(4)]
        public MOUSE_EVENT_RECORD MouseEvent;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEY_EVENT_RECORD
    {
        public int bKeyDown;
        public ushort wRepeatCount;
        public ushort wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public ushort UnicodeChar;
        public uint dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSE_EVENT_RECORD
    {
        public COORD dwMousePosition;
        public uint dwButtonState;
        public uint dwControlKeyState;
        public uint dwEventFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct COORD
    {
        public short X;
        public short Y;
    }

    internal static void EnableVirtualTerminal()
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

    internal static (IntPtr handle, uint previousMode) EnableMouseInput()
    {
        try
        {
            var consoleInputHandle = GetStdHandle(-10); // STD_INPUT_HANDLE
            if (GetConsoleMode(consoleInputHandle, out uint previousMode))
            {
                uint mode = previousMode;
                mode |= 0x0010; // ENABLE_MOUSE_INPUT
                mode &= ~0x0040u; // DISABLE_QUICK_EDIT_MODE
                mode |= 0x0080; // ENABLE_EXTENDED_FLAGS
                SetConsoleMode(consoleInputHandle, mode);
                return (consoleInputHandle, previousMode);
            }
        }
        catch
        {
            // Ignore errors
        }

        return (IntPtr.Zero, 0);
    }

    internal static void RestoreConsoleMode(IntPtr handle, uint mode)
    {
        if (handle != IntPtr.Zero)
        {
            try
            {
                SetConsoleMode(handle, mode);
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}
