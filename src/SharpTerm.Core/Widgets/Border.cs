namespace SharpTerm.Core.Widgets;

public enum BorderStyle
{
    Single,
    Double,
    Rounded
}

internal record BorderChars(char TopLeft, char TopRight, char BottomLeft, 
    char BottomRight, char Horizontal, char Vertical);

/// <summary>
/// A widget that renders a border/box.
/// </summary>
public class Border : Widget
{
    public BorderStyle Style { get; set; } = BorderStyle.Single;
    public string Title { get; set; } = string.Empty;
    
    public override void Render(ITerminalDriver driver)
    {
        if (!Visible) return;
        
        var chars = GetBorderChars(Style);
        
        // Fill interior with background color first
        for (int y = 1; y < Bounds.Height - 1; y++)
        {
            driver.SetCursorPosition(Bounds.X + 1, Bounds.Y + y);
            driver.Write(new string(' ', Math.Max(0, Bounds.Width - 2)), ForegroundColor, BackgroundColor);
        }
        
        // Top border
        driver.SetCursorPosition(Bounds.X, Bounds.Y);
        driver.Write(chars.TopLeft.ToString(), ForegroundColor, BackgroundColor);
        
        var topLine = new string(chars.Horizontal, Math.Max(0, Bounds.Width - 2));
        if (!string.IsNullOrEmpty(Title) && Title.Length < Bounds.Width - 4)
        {
            var titleText = $" {Title} ";
            topLine = titleText + new string(chars.Horizontal, Math.Max(0, Bounds.Width - titleText.Length - 2));
        }
        driver.Write(topLine, ForegroundColor, BackgroundColor);
        driver.Write(chars.TopRight.ToString(), ForegroundColor, BackgroundColor);
        
        // Sides
        for (int y = 1; y < Bounds.Height - 1; y++)
        {
            driver.SetCursorPosition(Bounds.X, Bounds.Y + y);
            driver.Write(chars.Vertical.ToString(), ForegroundColor, BackgroundColor);
            driver.SetCursorPosition(Bounds.X + Bounds.Width - 1, Bounds.Y + y);
            driver.Write(chars.Vertical.ToString(), ForegroundColor, BackgroundColor);
        }
        
        // Bottom border
        driver.SetCursorPosition(Bounds.X, Bounds.Y + Bounds.Height - 1);
        driver.Write(chars.BottomLeft.ToString(), ForegroundColor, BackgroundColor);
        driver.Write(new string(chars.Horizontal, Math.Max(0, Bounds.Width - 2)), ForegroundColor, BackgroundColor);
        driver.Write(chars.BottomRight.ToString(), ForegroundColor, BackgroundColor);
    }
    
    private static BorderChars GetBorderChars(BorderStyle style) => style switch
    {
        BorderStyle.Double => new('╔', '╗', '╚', '╝', '═', '║'),
        BorderStyle.Rounded => new('╭', '╮', '╰', '╯', '─', '│'),
        _ => new('┌', '┐', '└', '┘', '─', '│')
    };
}
