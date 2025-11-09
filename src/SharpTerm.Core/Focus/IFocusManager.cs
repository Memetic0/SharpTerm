namespace SharpTerm.Core.Focus;

/// <summary>
/// Manages focus navigation between widgets.
/// </summary>
public interface IFocusManager
{
    /// <summary>
    /// Gets the currently focused widget.
    /// </summary>
    Widget? FocusedWidget { get; }

    /// <summary>
    /// Gets or sets the focus navigation strategy.
    /// </summary>
    IFocusNavigationStrategy NavigationStrategy { get; set; }

    /// <summary>
    /// Sets focus to the specified widget.
    /// </summary>
    /// <param name="widget">The widget to focus.</param>
    /// <returns>True if focus was successfully set.</returns>
    bool SetFocus(Widget? widget);

    /// <summary>
    /// Moves focus to the next focusable widget.
    /// </summary>
    /// <returns>True if focus was moved.</returns>
    bool FocusNext();

    /// <summary>
    /// Moves focus to the previous focusable widget.
    /// </summary>
    /// <returns>True if focus was moved.</returns>
    bool FocusPrevious();

    /// <summary>
    /// Clears the current focus.
    /// </summary>
    void ClearFocus();

    /// <summary>
    /// Determines if the specified widget can receive focus.
    /// </summary>
    bool CanFocus(Widget widget);

    /// <summary>
    /// Registers a widget as focusable.
    /// </summary>
    void RegisterFocusable(Widget widget);

    /// <summary>
    /// Unregisters a focusable widget.
    /// </summary>
    void UnregisterFocusable(Widget widget);

    /// <summary>
    /// Event raised when focus changes.
    /// </summary>
    event EventHandler<FocusChangedEventArgs>? FocusChanged;
}

/// <summary>
/// Event arguments for focus changes.
/// </summary>
public class FocusChangedEventArgs : EventArgs
{
    public Widget? OldFocus { get; }
    public Widget? NewFocus { get; }

    public FocusChangedEventArgs(Widget? oldFocus, Widget? newFocus)
    {
        OldFocus = oldFocus;
        NewFocus = newFocus;
    }
}
