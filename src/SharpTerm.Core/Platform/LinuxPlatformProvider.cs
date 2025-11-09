namespace SharpTerm.Core.Platform;

/// <summary>
/// Linux-specific platform provider.
/// </summary>
internal class LinuxPlatformProvider : IPlatformProvider
{
    public string PlatformName => "Linux";

    public void EnableVirtualTerminal()
    {
        // Linux terminals natively support ANSI escape sequences
        // No action needed
    }

    public (IntPtr Handle, uint PreviousMode) EnableMouseInput()
    {
        // TODO: Implement Linux mouse support using terminal escape sequences
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
