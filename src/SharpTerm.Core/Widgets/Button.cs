namespace SharpTerm.Core.Widgets;

/// <summary>
/// A clickable button widget.
/// </summary>
public class Button : Widget
{
    public string Text { get; set; } = "Button";
    public bool IsFocused { get; set; }
    
    /// <summary>
    /// Gets or sets the key that invokes the button. Default is Enter.
    /// </summary>
    public ConsoleKey InvokeKey { get; set; } = ConsoleKey.Enter;
    
    public event EventHandler? Click;
    
    public override void Render(ITerminalDriver driver)
    {
        if (!Visible) return;
        
        var bg = IsFocused ? Color.Blue : BackgroundColor;
        var fg = IsFocused ? Color.White : ForegroundColor;
        
        driver.SetCursorPosition(Bounds.X, Bounds.Y);
        var buttonText = $"[ {Text} ]";
        
        // Center the button text within the available width
        string displayText;
        if (buttonText.Length >= Bounds.Width)
        {
            displayText = buttonText.Substring(0, Bounds.Width);
        }
        else
        {
            int padding = (Bounds.Width - buttonText.Length) / 2;
            displayText = new string(' ', padding) + buttonText + new string(' ', Bounds.Width - buttonText.Length - padding);
        }
        
        driver.Write(displayText, fg, bg);
    }
    
    public override bool HandleKey(ConsoleKeyInfo key)
    {
        if (!IsFocused) return false;
        
        if (key.Key == InvokeKey)
        {
            Click?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Programmatically invokes the button click.
    /// </summary>
    internal void InvokeClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
    }
}
