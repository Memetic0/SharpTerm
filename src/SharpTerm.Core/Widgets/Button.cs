namespace SharpTerm.Core.Widgets;

public enum ButtonStyle
{
    /// <summary>Bordered button with box drawing characters</summary>
    Boxed,

    /// <summary>Button with rounded corners</summary>
    Rounded,

    /// <summary>Simple brackets around text</summary>
    Simple,
}

/// <summary>
/// A clickable button widget.
/// </summary>
public class Button : Widget
{
    public string Text { get; set; } = "Button";
    public bool IsFocused { get; set; }
    public ButtonStyle Style { get; set; } = ButtonStyle.Rounded;

    /// <summary>
    /// Gets or sets the key that invokes the button. Default is Enter.
    /// </summary>
    public ConsoleKey InvokeKey { get; set; } = ConsoleKey.Enter;

    public event EventHandler? Click;

    public override void Render(ITerminalDriver driver)
    {
        if (!Visible)
            return;

        // Don't render if bounds are invalid
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        // Enhanced color scheme for better visual feedback
        Color bg,
            fg,
            borderColor;
        if (IsFocused)
        {
            bg = Color.Blue;
            fg = Color.White;
            borderColor = Color.Cyan;
        }
        else
        {
            bg = BackgroundColor;
            fg = ForegroundColor;
            borderColor = Color.DarkGray;
        }

        driver.SetCursorPosition(Bounds.X, Bounds.Y);

        string displayText;
        switch (Style)
        {
            case ButtonStyle.Boxed:
                // Box drawing style: ┌─Text─┐
                if (Bounds.Width >= Text.Length + 4)
                {
                    int innerWidth = Bounds.Width - 2;
                    int textPadding = (innerWidth - Text.Length) / 2;
                    var leftPad = new string(' ', textPadding);
                    var rightPad = new string(' ', innerWidth - Text.Length - textPadding);
                    displayText = $"┤{leftPad}{Text}{rightPad}├";
                }
                else
                {
                    displayText = Text.PadRight(Bounds.Width);
                }
                driver.Write(displayText, fg, bg);
                break;

            case ButtonStyle.Rounded:
                // Rounded style: ╭─Text─╮
                if (Bounds.Width >= Text.Length + 4)
                {
                    int innerWidth = Bounds.Width - 2;
                    int textPadding = (innerWidth - Text.Length) / 2;
                    var leftPad = new string(' ', textPadding);
                    var rightPad = new string(' ', innerWidth - Text.Length - textPadding);
                    displayText = $"({leftPad}{Text}{rightPad})";
                }
                else
                {
                    displayText = Text.PadRight(Bounds.Width);
                }
                driver.Write(displayText, fg, bg);
                break;

            case ButtonStyle.Simple:
            default:
                // Simple brackets: [ Text ]
                var buttonText = $"[ {Text} ]";
                if (buttonText.Length >= Bounds.Width)
                {
                    displayText = buttonText.Substring(0, Bounds.Width);
                }
                else
                {
                    int padding = (Bounds.Width - buttonText.Length) / 2;
                    displayText =
                        new string(' ', padding)
                        + buttonText
                        + new string(' ', Bounds.Width - buttonText.Length - padding);
                }
                driver.Write(displayText, fg, bg);
                break;
        }
    }

    public override bool HandleKey(ConsoleKeyInfo key)
    {
        if (!IsFocused)
            return false;

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
