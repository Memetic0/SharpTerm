namespace SharpTerm.Core.Services;

/// <summary>
/// Container for application-level services.
/// </summary>
public interface IApplicationServices
{
    /// <summary>
    /// Gets the terminal driver service.
    /// </summary>
    ITerminalDriver TerminalDriver { get; }

    /// <summary>
    /// Gets the theme manager service.
    /// </summary>
    Theming.ThemeManager ThemeManager { get; }

    /// <summary>
    /// Gets the platform provider service.
    /// </summary>
    IPlatformProvider PlatformProvider { get; }
}

/// <summary>
/// Default implementation of application services.
/// </summary>
public class ApplicationServices : IApplicationServices
{
    public ApplicationServices(
        ITerminalDriver? terminalDriver = null,
        Theming.ThemeManager? themeManager = null,
        IPlatformProvider? platformProvider = null)
    {
        PlatformProvider = platformProvider ?? Core.PlatformProvider.Create();
        TerminalDriver = terminalDriver ?? new AnsiTerminalDriver(PlatformProvider);
        ThemeManager = themeManager ?? new Theming.ThemeManager();
    }

    public ITerminalDriver TerminalDriver { get; }
    public Theming.ThemeManager ThemeManager { get; }
    public IPlatformProvider PlatformProvider { get; }
}
