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

    internal static void RenderDirtyWidgets(ITerminalDriver driver, HashSet<Widget> dirtyWidgets)
    {
        Widget[] dirtyArray;
        lock (dirtyWidgets)
        {
            if (dirtyWidgets.Count == 0)
                return;

            dirtyArray = dirtyWidgets.ToArray();
            dirtyWidgets.Clear();
        }

        // Get current terminal size
        int termWidth = driver.Width;
        int termHeight = driver.Height;

        foreach (var widget in dirtyArray)
        {
            // Only render if widget is within terminal bounds
            if (widget.Bounds.X < termWidth && widget.Bounds.Y < termHeight)
            {
                widget.Render(driver);
            }
        }
        driver.Flush();
    }
}
