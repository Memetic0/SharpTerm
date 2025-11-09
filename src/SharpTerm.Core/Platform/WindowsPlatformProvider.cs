using System.Runtime.InteropServices;
using SharpTerm.Core.DriverLogic;

namespace SharpTerm.Core.Platform;

/// <summary>
/// Windows-specific platform provider using Windows Console API.
/// </summary>
internal class WindowsPlatformProvider : IPlatformProvider
{
    public string PlatformName => "Windows";

    public void EnableVirtualTerminal()
    {
        WindowsConsole.EnableVirtualTerminal();
    }

    public (IntPtr Handle, uint PreviousMode) EnableMouseInput()
    {
        return WindowsConsole.EnableMouseInput();
    }

    public void RestoreConsoleMode(IntPtr handle, uint mode)
    {
        WindowsConsole.RestoreConsoleMode(handle, mode);
    }

    public bool HasInputEvents(IntPtr consoleInputHandle)
    {
        if (consoleInputHandle == IntPtr.Zero)
            return Console.KeyAvailable;

        try
        {
            if (WindowsConsole.GetNumberOfConsoleInputEvents(consoleInputHandle, out uint numEvents))
            {
                return numEvents > 0;
            }
        }
        catch
        {
            // Fall back to KeyAvailable
        }

        return Console.KeyAvailable;
    }

    public ConsoleEventType ReadConsoleEvent(
        IntPtr consoleInputHandle,
        out ConsoleKeyInfo? keyInfo,
        out MouseEvent? mouseEvent)
    {
        return ConsoleInputReader.ReadConsoleEvent(consoleInputHandle, out keyInfo, out mouseEvent);
    }
}
