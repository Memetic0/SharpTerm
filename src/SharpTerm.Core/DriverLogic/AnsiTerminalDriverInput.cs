using static SharpTerm.Core.DriverLogic.WindowsConsole;

namespace SharpTerm.Core.DriverLogic;

/// <summary>
/// Input handling for terminal driver.
/// </summary>
internal static class ConsoleInputReader
{
    internal static ConsoleEventType ReadConsoleEvent(
        IntPtr consoleInputHandle,
        out ConsoleKeyInfo? keyInfo,
        out MouseEvent? mouseEvent
    )
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
            if (
                GetNumberOfConsoleInputEvents(consoleInputHandle, out uint numEvents)
                && numEvents > 0
            )
            {
                var buffer = new INPUT_RECORD[1];
                if (
                    ReadConsoleInput(consoleInputHandle, buffer, 1, out uint eventsRead)
                    && eventsRead > 0
                )
                {
                    var record = buffer[0];

                    if (record.EventType == 1) // KEY_EVENT
                    {
                        return ProcessKeyboardEvent(record.KeyEvent, out keyInfo);
                    }
                    else if (record.EventType == 2) // MOUSE_EVENT
                    {
                        return ProcessMouseEvent(record.MouseEvent, out mouseEvent);
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

    private static ConsoleEventType ProcessKeyboardEvent(
        KEY_EVENT_RECORD keyEvent,
        out ConsoleKeyInfo? keyInfo
    )
    {
        keyInfo = null;

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

    private static ConsoleEventType ProcessMouseEvent(
        MOUSE_EVENT_RECORD mouseRecord,
        out MouseEvent? mouseEvent
    )
    {
        mouseEvent = null;

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
}
