namespace SharpTerm.Core.Theming;

/// <summary>
/// Manages theme application across the application.
/// </summary>
public class ThemeManager
{
    private Theme _currentTheme;

    public ThemeManager()
    {
        _currentTheme = Theme.Dark;
    }

    /// <summary>
    /// Gets or sets the current theme.
    /// </summary>
    public Theme CurrentTheme
    {
        get => _currentTheme;
        set => _currentTheme = value ?? Theme.Dark;
    }

    /// <summary>
    /// Event raised when the theme changes.
    /// </summary>
    public event EventHandler<Theme>? ThemeChanged;

    /// <summary>
    /// Sets the current theme and raises the ThemeChanged event.
    /// </summary>
    public void SetTheme(Theme theme)
    {
        if (theme == null)
            throw new ArgumentNullException(nameof(theme));

        _currentTheme = theme;
        ThemeChanged?.Invoke(this, theme);
    }

    /// <summary>
    /// Applies the current theme to a widget and all its children.
    /// </summary>
    public void ApplyTheme(Widget widget)
    {
        if (widget == null)
            throw new ArgumentNullException(nameof(widget));

        _currentTheme.ApplyTo(widget);

        foreach (var child in widget.Children)
        {
            ApplyTheme(child);
        }
    }

    /// <summary>
    /// Applies the current theme to all widgets in a collection.
    /// </summary>
    public void ApplyTheme(IEnumerable<Widget> widgets)
    {
        foreach (var widget in widgets)
        {
            ApplyTheme(widget);
        }
    }
}
