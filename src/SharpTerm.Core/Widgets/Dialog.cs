namespace SharpTerm.Core.Widgets;

/// <summary>
/// A simple modal dialog box with title, message, and border.
/// </summary>
public class Dialog : Widget
{
    public string Title { get; set; } = "Dialog";
    public string Message { get; set; } = "";
    public BorderStyle Style { get; set; } = BorderStyle.Rounded;

    public override void Render(ITerminalDriver driver)
    {
        if (!Visible)
            return;

        // Draw shadow effect first
        for (int y = 1; y < Bounds.Height; y++)
        {
            driver.SetCursorPosition(Bounds.X + 2, Bounds.Y + y);
            driver.Write(new string(' ', Bounds.Width), Color.Black, Color.DarkGray);
        }

        // Draw border
        char topLeft, topRight, bottomLeft, bottomRight, horizontal, vertical;
        switch (Style)
        {
            case BorderStyle.Double:
                topLeft = '╔';
                topRight = '╗';
                bottomLeft = '╚';
                bottomRight = '╝';
                horizontal = '═';
                vertical = '║';
                break;
            case BorderStyle.Rounded:
                topLeft = '╭';
                topRight = '╮';
                bottomLeft = '╰';
                bottomRight = '╯';
                horizontal = '─';
                vertical = '│';
                break;
            case BorderStyle.Single:
            default:
                topLeft = '┌';
                topRight = '┐';
                bottomLeft = '└';
                bottomRight = '┘';
                horizontal = '─';
                vertical = '│';
                break;
        }

        // Top border with title
        driver.SetCursorPosition(Bounds.X, Bounds.Y);
        string titleText = $" {Title} ";
        int titlePadding = (Bounds.Width - 2 - titleText.Length) / 2;
        string topBorder = topLeft
            + new string(horizontal, titlePadding)
            + titleText
            + new string(horizontal, Bounds.Width - 2 - titlePadding - titleText.Length)
            + topRight;
        driver.Write(topBorder, ForegroundColor, BackgroundColor);

        // Sides and content
        string[] lines = Message.Split('\n');
        int contentStartY = Bounds.Y + 1;
        int contentHeight = Bounds.Height - 2;

        for (int i = 0; i < contentHeight; i++)
        {
            driver.SetCursorPosition(Bounds.X, contentStartY + i);
            driver.Write(vertical.ToString(), ForegroundColor, BackgroundColor);

            // Message content (centered)
            if (i < lines.Length)
            {
                string line = lines[i];
                if (line.Length > Bounds.Width - 4)
                    line = line.Substring(0, Bounds.Width - 4);

                int padding = (Bounds.Width - 2 - line.Length) / 2;
                string content =
                    new string(' ', padding)
                    + line
                    + new string(' ', Bounds.Width - 2 - padding - line.Length);
                driver.Write(content, Color.White, BackgroundColor);
            }
            else
            {
                driver.Write(new string(' ', Bounds.Width - 2), Color.White, BackgroundColor);
            }

            driver.Write(vertical.ToString(), ForegroundColor, BackgroundColor);
        }

        // Bottom border
        driver.SetCursorPosition(Bounds.X, Bounds.Y + Bounds.Height - 1);
        driver.Write(
            bottomLeft + new string(horizontal, Bounds.Width - 2) + bottomRight,
            ForegroundColor,
            BackgroundColor
        );
    }

    public override bool HandleKey(ConsoleKeyInfo key)
    {
        // Dialogs can be closed with ESC or Enter
        if (key.Key == ConsoleKey.Escape || key.Key == ConsoleKey.Enter)
        {
            Visible = false;
            OnChanged();
            return true;
        }
        return false;
    }
}
