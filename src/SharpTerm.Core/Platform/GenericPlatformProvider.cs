namespace SharpTerm.Core.Platform;

/// <summary>
/// Generic fallback platform provider for unsupported platforms.
/// </summary>
internal class GenericPlatformProvider : IPlatformProvider
{
    public string PlatformName => "Generic";

    public void EnableVirtualTerminal()
    {
        // Assume ANSI support is available
    }

    public (IntPtr Handle, uint PreviousMode) EnableMouseInput()
    {
        return (IntPtr.Zero, 0);
    }

    public void RestoreConsoleMode(IntPtr handle, uint mode)
    {
        // No action needed
    }

    public bool HasInputEvents(IntPtr consoleInputHandle)
    {
        return Console.KeyAvailable;
    }

    public ConsoleEventType ReadConsoleEvent(
        IntPtr consoleInputHandle,
        out ConsoleKeyInfo? keyInfo,
        out MouseEvent? mouseEvent)
    {
        keyInfo = null;
        mouseEvent = null;

        if (Console.KeyAvailable)
        {
            keyInfo = Console.ReadKey(true);
            return ConsoleEventType.Keyboard;
        }

        return ConsoleEventType.None;
    }
}
