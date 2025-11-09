namespace SharpTerm.Core.Widgets;

public enum BorderStyle
{
    Single,
    Double,
    Rounded,
}

internal record BorderChars(
    char TopLeft,
    char TopRight,
    char BottomLeft,
    char BottomRight,
    char Horizontal,
    char Vertical
);

/// <summary>
/// A widget that renders a border/box.
/// </summary>
public class Border : Widget
{
    public BorderStyle Style { get; set; } = BorderStyle.Single;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public bool ShowShadow { get; set; } = false;

    public override void Render(ITerminalDriver driver)
    {
        if (!Visible)
            return;

        // Don't render if bounds are too small
        if (Bounds.Width < 2 || Bounds.Height < 2)
            return;

        var chars = GetBorderChars(Style);

        // Fill interior with background color first
        for (int y = 1; y < Bounds.Height - 1; y++)
        {
            int interiorWidth = Bounds.Width - 2;
            if (interiorWidth > 0)
            {
                driver.SetCursorPosition(Bounds.X + 1, Bounds.Y + y);
                driver.Write(new string(' ', interiorWidth), ForegroundColor, BackgroundColor);
            }
        }

        // Top border
        driver.SetCursorPosition(Bounds.X, Bounds.Y);
        driver.Write(chars.TopLeft.ToString(), ForegroundColor, BackgroundColor);

        var topLine = new string(chars.Horizontal, Math.Max(0, Bounds.Width - 2));
        if (!string.IsNullOrEmpty(Title) && Title.Length < Bounds.Width - 4)
        {
            var titleText = $" {Title} ";
            topLine =
                titleText
                + new string(chars.Horizontal, Math.Max(0, Bounds.Width - titleText.Length - 2));
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

        var bottomLine = new string(chars.Horizontal, Math.Max(0, Bounds.Width - 2));
        if (!string.IsNullOrEmpty(Subtitle) && Subtitle.Length < Bounds.Width - 4)
        {
            var subtitleText = $" {Subtitle} ";
            bottomLine =
                subtitleText
                + new string(chars.Horizontal, Math.Max(0, Bounds.Width - subtitleText.Length - 2));
        }
        driver.Write(bottomLine, ForegroundColor, BackgroundColor);
        driver.Write(chars.BottomRight.ToString(), ForegroundColor, BackgroundColor);

        // Draw shadow if enabled
        if (ShowShadow && Bounds.X + Bounds.Width < 100 && Bounds.Y + Bounds.Height < 50) // reasonable bounds
        {
            // Bottom shadow
            if (Bounds.Y + Bounds.Height < 50)
            {
                driver.SetCursorPosition(Bounds.X + 1, Bounds.Y + Bounds.Height);
                driver.Write(
                    new string('░', Math.Min(Bounds.Width, 100 - Bounds.X - 1)),
                    Color.DarkGray,
                    Color.Black
                );
            }
            // Right shadow
            for (int y = 1; y < Bounds.Height && Bounds.Y + y < 50; y++)
            {
                if (Bounds.X + Bounds.Width < 100)
                {
                    driver.SetCursorPosition(Bounds.X + Bounds.Width, Bounds.Y + y);
                    driver.Write("░", Color.DarkGray, Color.Black);
                }
            }
        }
    }

    private static BorderChars GetBorderChars(BorderStyle style) =>
        style switch
        {
            BorderStyle.Double => new('╔', '╗', '╚', '╝', '═', '║'),
            BorderStyle.Rounded => new('╭', '╮', '╰', '╯', '─', '│'),
            _ => new('┌', '┐', '└', '┘', '─', '│'),
        };
}
