namespace SharpTerm.Core;

/// <summary>
/// Provides platform-specific functionality abstraction.
/// </summary>
public interface IPlatformProvider
{
    /// <summary>
    /// Gets the platform name.
    /// </summary>
    string PlatformName { get; }

    /// <summary>
    /// Enables virtual terminal processing for ANSI escape sequences.
    /// </summary>
    void EnableVirtualTerminal();

    /// <summary>
    /// Enables mouse input support.
    /// </summary>
    /// <returns>Handle to console input and previous console mode.</returns>
    (IntPtr Handle, uint PreviousMode) EnableMouseInput();

    /// <summary>
    /// Restores console mode.
    /// </summary>
    void RestoreConsoleMode(IntPtr handle, uint mode);

    /// <summary>
    /// Checks if mouse input events are available.
    /// </summary>
    bool HasInputEvents(IntPtr consoleInputHandle);

    /// <summary>
    /// Reads a console event (keyboard, mouse, etc.).
    /// </summary>
    ConsoleEventType ReadConsoleEvent(
        IntPtr consoleInputHandle,
        out ConsoleKeyInfo? keyInfo,
        out MouseEvent? mouseEvent);
}

/// <summary>
/// Factory for creating platform-specific providers.
/// </summary>
public static class PlatformProvider
{
    public static IPlatformProvider Create()
    {
        if (OperatingSystem.IsWindows())
        {
            return new Platform.WindowsPlatformProvider();
        }
        else if (OperatingSystem.IsLinux())
        {
            return new Platform.LinuxPlatformProvider();
        }
        else if (OperatingSystem.IsMacOS())
        {
            return new Platform.MacOSPlatformProvider();
        }
        else
        {
            return new Platform.GenericPlatformProvider();
        }
    }
}
