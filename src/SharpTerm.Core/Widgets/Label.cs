using SharpTerm.Core.Performance;

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

        // Use zero-allocation text processing with SpanHelpers
        ReadOnlySpan<char> textSpan = Text.AsSpan();
        var lineEnumerator = TextProcessor.EnumerateLines(textSpan);

        int lineIndex = 0;
        foreach (var line in lineEnumerator)
        {
            if (lineIndex >= Bounds.Height)
                break;

            driver.SetCursorPosition(Bounds.X, Bounds.Y + lineIndex);

            // Truncate line if needed
            ReadOnlySpan<char> displayLine = line.Length > Bounds.Width
                ? line[..Bounds.Width]
                : line;

            // Apply alignment using SpanHelpers (optimized string.Create internally)
            string finalLine = Alignment switch
            {
                TextAlignment.Center => SpanHelpers.Center(displayLine, Bounds.Width),
                TextAlignment.Right => SpanHelpers.PadLeft(displayLine, Bounds.Width),
                _ => SpanHelpers.PadRight(displayLine, Bounds.Width)
            };

            driver.Write(finalLine, ForegroundColor, BackgroundColor);
            lineIndex++;
        }

        // Fill remaining lines with background color using SpanHelpers
        string emptyLine = SpanHelpers.Repeat(' ', Bounds.Width);

        for (int i = lineIndex; i < Bounds.Height; i++)
        {
            driver.SetCursorPosition(Bounds.X, Bounds.Y + i);
            driver.Write(emptyLine, ForegroundColor, BackgroundColor);
        }
    }
}
