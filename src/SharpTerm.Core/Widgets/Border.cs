using SharpTerm.Core.Performance;

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

        // Use SpanHelpers for efficient string creation
        int interiorWidth = Bounds.Width - 2;
        string interiorFill = interiorWidth > 0 ? SpanHelpers.Repeat(' ', interiorWidth) : string.Empty;

        // Fill interior with background color first
        for (int y = 1; y < Bounds.Height - 1; y++)
        {
            if (interiorWidth > 0)
            {
                driver.SetCursorPosition(Bounds.X + 1, Bounds.Y + y);
                driver.Write(interiorFill, ForegroundColor, BackgroundColor);
            }
        }

        // Top border with ValueStringBuilder for efficient construction
        Span<char> stackBuffer = stackalloc char[256];
        var vsb = Bounds.Width <= 256
            ? new ValueStringBuilder(stackBuffer)
            : new ValueStringBuilder(Bounds.Width);

        try
        {
            // Build top line
            vsb.Append(chars.TopLeft);
            if (!string.IsNullOrEmpty(Title) && Title.Length < Bounds.Width - 4)
            {
                vsb.Append(' ');
                vsb.Append(Title);
                vsb.Append(' ');
                vsb.Append(chars.Horizontal, Math.Max(0, Bounds.Width - Title.Length - 4));
            }
            else
            {
                vsb.Append(chars.Horizontal, Math.Max(0, Bounds.Width - 2));
            }
            vsb.Append(chars.TopRight);

            driver.SetCursorPosition(Bounds.X, Bounds.Y);
            driver.Write(vsb.ToString(), ForegroundColor, BackgroundColor);

            // Sides
            string verticalChar = chars.Vertical.ToString();
            for (int y = 1; y < Bounds.Height - 1; y++)
            {
                driver.SetCursorPosition(Bounds.X, Bounds.Y + y);
                driver.Write(verticalChar, ForegroundColor, BackgroundColor);
                driver.SetCursorPosition(Bounds.X + Bounds.Width - 1, Bounds.Y + y);
                driver.Write(verticalChar, ForegroundColor, BackgroundColor);
            }

            // Bottom border
            vsb.Length = 0; // Reuse builder
            vsb.Append(chars.BottomLeft);
            if (!string.IsNullOrEmpty(Subtitle) && Subtitle.Length < Bounds.Width - 4)
            {
                vsb.Append(' ');
                vsb.Append(Subtitle);
                vsb.Append(' ');
                vsb.Append(chars.Horizontal, Math.Max(0, Bounds.Width - Subtitle.Length - 4));
            }
            else
            {
                vsb.Append(chars.Horizontal, Math.Max(0, Bounds.Width - 2));
            }
            vsb.Append(chars.BottomRight);

            driver.SetCursorPosition(Bounds.X, Bounds.Y + Bounds.Height - 1);
            driver.Write(vsb.ToString(), ForegroundColor, BackgroundColor);

            // Draw shadow if enabled
            if (ShowShadow && Bounds.X + Bounds.Width < 100 && Bounds.Y + Bounds.Height < 50)
            {
                string shadowChar = "░";
                // Bottom shadow
                if (Bounds.Y + Bounds.Height < 50)
                {
                    driver.SetCursorPosition(Bounds.X + 1, Bounds.Y + Bounds.Height);
                    string bottomShadow = SpanHelpers.Repeat('░', Math.Min(Bounds.Width, 100 - Bounds.X - 1));
                    driver.Write(bottomShadow, Color.DarkGray, Color.Black);
                }
                // Right shadow
                for (int y = 1; y < Bounds.Height && Bounds.Y + y < 50; y++)
                {
                    if (Bounds.X + Bounds.Width < 100)
                    {
                        driver.SetCursorPosition(Bounds.X + Bounds.Width, Bounds.Y + y);
                        driver.Write(shadowChar, Color.DarkGray, Color.Black);
                    }
                }
            }
        }
        finally
        {
            vsb.Dispose();
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
