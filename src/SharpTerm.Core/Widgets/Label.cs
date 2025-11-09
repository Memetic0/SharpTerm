namespace SharpTerm.Core.Widgets;

public enum TextAlignment
{
    Left,
    Center,
    Right,
}

/// <summary>
/// A widget that displays text.
/// </summary>
public class Label : Widget
{
    public string Text { get; set; } = string.Empty;
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;

    public override void Render(ITerminalDriver driver)
    {
        if (!Visible)
            return;

        // Don't render if bounds are invalid
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var lines = Text.Split('\n');

        // Render each line within bounds
        for (int i = 0; i < Bounds.Height; i++)
        {
            driver.SetCursorPosition(Bounds.X, Bounds.Y + i);

            if (i < lines.Length)
            {
                var line = lines[i];
                if (line.Length > Bounds.Width)
                {
                    line = line.Substring(0, Bounds.Width);
                }

                // Apply alignment by adding padding
                var displayLine = Alignment switch
                {
                    TextAlignment.Center => line.PadLeft((Bounds.Width + line.Length) / 2)
                        .PadRight(Bounds.Width),
                    TextAlignment.Right => line.PadLeft(Bounds.Width),
                    _ => line.PadRight(Bounds.Width),
                };

                driver.Write(displayLine, ForegroundColor, BackgroundColor);
            }
            else
            {
                // Fill empty lines with background color
                driver.Write(new string(' ', Bounds.Width), ForegroundColor, BackgroundColor);
            }
        }
    }
}
