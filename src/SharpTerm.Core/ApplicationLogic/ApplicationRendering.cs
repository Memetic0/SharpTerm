namespace SharpTerm.Core.ApplicationLogic;

/// <summary>
/// Application rendering logic.
/// </summary>
internal static class Renderer
{
    internal static void RenderAll(ITerminalDriver driver, List<Widget> widgets)
    {
        driver.Clear();

        // Get current terminal size
        int termWidth = driver.Width;
        int termHeight = driver.Height;

        foreach (var widget in widgets)
        {
            // Only render if widget is within terminal bounds
            if (widget.Bounds.X < termWidth && widget.Bounds.Y < termHeight)
            {
                widget.Render(driver);
            }
        }
        driver.Flush();
    }

    internal static void RenderDirtyWidgets(ITerminalDriver driver, IEnumerable<Widget> dirtyWidgets)
    {
        // Get current terminal size
        int termWidth = driver.Width;
        int termHeight = driver.Height;

        bool anyRendered = false;
        foreach (var widget in dirtyWidgets)
        {
            // Only render if widget is within terminal bounds
            if (widget.Bounds.X < termWidth && widget.Bounds.Y < termHeight)
            {
                widget.Render(driver);
                anyRendered = true;
            }
        }

        if (anyRendered)
        {
            driver.Flush();
        }
    }
}
