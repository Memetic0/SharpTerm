namespace SharpTerm.Core.Layout;

/// <summary>
/// Defines a layout manager that arranges child widgets within a container.
/// </summary>
public interface ILayoutManager
{
    /// <summary>
    /// Measures the desired size of the layout given the available space.
    /// </summary>
    /// <param name="availableWidth">Available width for the layout.</param>
    /// <param name="availableHeight">Available height for the layout.</param>
    /// <returns>The desired size of the layout.</returns>
    (int Width, int Height) Measure(int availableWidth, int availableHeight);

    /// <summary>
    /// Arranges child widgets within the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounds within which to arrange children.</param>
    void Arrange(Rectangle bounds);

    /// <summary>
    /// Adds a child widget to the layout.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    void AddChild(Widget widget);

    /// <summary>
    /// Removes a child widget from the layout.
    /// </summary>
    /// <param name="widget">The widget to remove.</param>
    void RemoveChild(Widget widget);

    /// <summary>
    /// Gets all child widgets managed by this layout.
    /// </summary>
    IReadOnlyList<Widget> Children { get; }
}
