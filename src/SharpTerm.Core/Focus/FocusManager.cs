namespace SharpTerm.Core.Focus;

/// <summary>
/// Default implementation of IFocusManager.
/// </summary>
public class FocusManager : IFocusManager
{
    private Widget? _focusedWidget;
    private readonly HashSet<Widget> _focusableWidgets = new();
    private IFocusNavigationStrategy _navigationStrategy = new TabOrderNavigationStrategy();

    public Widget? FocusedWidget => _focusedWidget;

    public IFocusNavigationStrategy NavigationStrategy
    {
        get => _navigationStrategy;
        set => _navigationStrategy = value ?? throw new ArgumentNullException(nameof(value));
    }

    public event EventHandler<FocusChangedEventArgs>? FocusChanged;

    public bool SetFocus(Widget? widget)
    {
        if (widget != null && !CanFocus(widget))
            return false;

        if (_focusedWidget == widget)
            return true;

        var oldFocus = _focusedWidget;

        // Clear old focus
        if (_focusedWidget != null)
        {
            SetWidgetFocus(_focusedWidget, false);
        }

        // Set new focus
        _focusedWidget = widget;
        if (_focusedWidget != null)
        {
            SetWidgetFocus(_focusedWidget, true);
        }

        FocusChanged?.Invoke(this, new FocusChangedEventArgs(oldFocus, _focusedWidget));
        return true;
    }

    public bool FocusNext()
    {
        var next = _navigationStrategy.GetNext(_focusedWidget, _focusableWidgets.Where(w => w.Visible));
        return SetFocus(next);
    }

    public bool FocusPrevious()
    {
        var previous = _navigationStrategy.GetPrevious(_focusedWidget, _focusableWidgets.Where(w => w.Visible));
        return SetFocus(previous);
    }

    public void ClearFocus()
    {
        SetFocus(null);
    }

    public bool CanFocus(Widget widget)
    {
        return _focusableWidgets.Contains(widget) && widget.Visible;
    }

    public void RegisterFocusable(Widget widget)
    {
        if (widget == null)
            throw new ArgumentNullException(nameof(widget));

        _focusableWidgets.Add(widget);
    }

    public void UnregisterFocusable(Widget widget)
    {
        if (_focusableWidgets.Remove(widget) && _focusedWidget == widget)
        {
            ClearFocus();
        }
    }

    private void SetWidgetFocus(Widget widget, bool isFocused)
    {
        // Use reflection to set IsFocused property if it exists
        var prop = widget.GetType().GetProperty("IsFocused");
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(widget, isFocused);
        }
    }
}
