namespace SharpTerm.Core;

/// <summary>
/// Base class for all UI widgets.
/// </summary>
public abstract class Widget
{
    /// <summary>
    /// Event raised when the widget's visual state changes and needs to be redrawn.
    /// </summary>
    public event EventHandler? Changed;
    
    /// <summary>
    /// Raises the Changed event to notify that the widget needs to be redrawn.
    /// </summary>
    protected void OnChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }
    
    public Rectangle Bounds { get; set; }
    public bool Visible { get; set; } = true;
    public Color ForegroundColor { get; set; } = Color.White;
    public Color BackgroundColor { get; set; } = Color.Transparent;
    
    /// <summary>
    /// Renders the widget to the terminal driver.
    /// </summary>
    public abstract void Render(ITerminalDriver driver);
    
    /// <summary>
    /// Handles a key press. Returns true if the key was handled.
    /// </summary>
    public virtual bool HandleKey(ConsoleKeyInfo key) => false;
}
