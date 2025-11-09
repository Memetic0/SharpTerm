namespace SharpTerm.Core.Platform;

/// <summary>
/// macOS-specific platform provider.
/// </summary>
internal class MacOSPlatformProvider : IPlatformProvider
{
    public string PlatformName => "macOS";

    public void EnableVirtualTerminal()
    {
        // macOS terminals natively support ANSI escape sequences
        // No action needed
    }

    public (IntPtr Handle, uint PreviousMode) EnableMouseInput()
    {
        // TODO: Implement macOS mouse support using terminal escape sequences
        // For now, return default values
        return (IntPtr.Zero, 0);
    }

    public void RestoreConsoleMode(IntPtr handle, uint mode)
    {
        // No action needed for now
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
