namespace SharpTerm.Core.Focus;

/// <summary>
/// Strategy for determining focus navigation order.
/// </summary>
public interface IFocusNavigationStrategy
{
    /// <summary>
    /// Gets the next focusable widget from the current widget.
    /// </summary>
    Widget? GetNext(Widget? current, IEnumerable<Widget> focusableWidgets);

    /// <summary>
    /// Gets the previous focusable widget from the current widget.
    /// </summary>
    Widget? GetPrevious(Widget? current, IEnumerable<Widget> focusableWidgets);

    /// <summary>
    /// Gets the first focusable widget.
    /// </summary>
    Widget? GetFirst(IEnumerable<Widget> focusableWidgets);
}

/// <summary>
/// Tab-order navigation strategy (sequential).
/// </summary>
public class TabOrderNavigationStrategy : IFocusNavigationStrategy
{
    public Widget? GetNext(Widget? current, IEnumerable<Widget> focusableWidgets)
    {
        var widgets = focusableWidgets.ToList();
        if (widgets.Count == 0) return null;

        if (current == null) return widgets.FirstOrDefault();

        var currentIndex = widgets.IndexOf(current);
        if (currentIndex == -1) return widgets.FirstOrDefault();

        return widgets[(currentIndex + 1) % widgets.Count];
    }

    public Widget? GetPrevious(Widget? current, IEnumerable<Widget> focusableWidgets)
    {
        var widgets = focusableWidgets.ToList();
        if (widgets.Count == 0) return null;

        if (current == null) return widgets.LastOrDefault();

        var currentIndex = widgets.IndexOf(current);
        if (currentIndex == -1) return widgets.LastOrDefault();

        return widgets[(currentIndex - 1 + widgets.Count) % widgets.Count];
    }

    public Widget? GetFirst(IEnumerable<Widget> focusableWidgets)
    {
        return focusableWidgets.FirstOrDefault();
    }
}

/// <summary>
/// Spatial navigation strategy (based on widget positions).
/// </summary>
public class SpatialNavigationStrategy : IFocusNavigationStrategy
{
    public Widget? GetNext(Widget? current, IEnumerable<Widget> focusableWidgets)
    {
        // Navigate to the nearest widget to the right or below
        if (current == null) return GetFirst(focusableWidgets);

        var candidates = focusableWidgets
            .Where(w => w != current)
            .Where(w => w.Bounds.Y > current.Bounds.Y ||
                       (w.Bounds.Y == current.Bounds.Y && w.Bounds.X > current.Bounds.X))
            .OrderBy(w => w.Bounds.Y)
            .ThenBy(w => w.Bounds.X)
            .ToList();

        return candidates.FirstOrDefault() ?? GetFirst(focusableWidgets);
    }

    public Widget? GetPrevious(Widget? current, IEnumerable<Widget> focusableWidgets)
    {
        // Navigate to the nearest widget to the left or above
        if (current == null) return focusableWidgets.LastOrDefault();

        var candidates = focusableWidgets
            .Where(w => w != current)
            .Where(w => w.Bounds.Y < current.Bounds.Y ||
                       (w.Bounds.Y == current.Bounds.Y && w.Bounds.X < current.Bounds.X))
            .OrderByDescending(w => w.Bounds.Y)
            .ThenByDescending(w => w.Bounds.X)
            .ToList();

        return candidates.FirstOrDefault() ?? focusableWidgets.LastOrDefault();
    }

    public Widget? GetFirst(IEnumerable<Widget> focusableWidgets)
    {
        return focusableWidgets
            .OrderBy(w => w.Bounds.Y)
            .ThenBy(w => w.Bounds.X)
            .FirstOrDefault();
    }
}
