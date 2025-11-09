namespace SharpTerm.Core.Layout;

/// <summary>
/// Orientation for stack layout.
/// </summary>
public enum StackOrientation
{
    Vertical,
    Horizontal
}

/// <summary>
/// Arranges child widgets in a vertical or horizontal stack.
/// </summary>
public class StackLayout : ILayoutManager
{
    private readonly List<Widget> _children = new();
    private int _spacing;

    public StackOrientation Orientation { get; set; } = StackOrientation.Vertical;

    /// <summary>
    /// Gets or sets the spacing between child widgets.
    /// </summary>
    public int Spacing
    {
        get => _spacing;
        set => _spacing = Math.Max(0, value);
    }

    public IReadOnlyList<Widget> Children => _children.AsReadOnly();

    public void AddChild(Widget widget)
    {
        if (!_children.Contains(widget))
        {
            _children.Add(widget);
        }
    }

    public void RemoveChild(Widget widget)
    {
        _children.Remove(widget);
    }

    public (int Width, int Height) Measure(int availableWidth, int availableHeight)
    {
        if (_children.Count == 0)
            return (0, 0);

        int totalWidth = 0;
        int totalHeight = 0;

        if (Orientation == StackOrientation.Vertical)
        {
            foreach (var child in _children.Where(c => c.Visible))
            {
                totalWidth = Math.Max(totalWidth, child.Bounds.Width);
                totalHeight += child.Bounds.Height;
            }
            // Add spacing between visible children
            int visibleCount = _children.Count(c => c.Visible);
            if (visibleCount > 1)
                totalHeight += Spacing * (visibleCount - 1);
        }
        else // Horizontal
        {
            foreach (var child in _children.Where(c => c.Visible))
            {
                totalWidth += child.Bounds.Width;
                totalHeight = Math.Max(totalHeight, child.Bounds.Height);
            }
            // Add spacing between visible children
            int visibleCount = _children.Count(c => c.Visible);
            if (visibleCount > 1)
                totalWidth += Spacing * (visibleCount - 1);
        }

        return (Math.Min(totalWidth, availableWidth), Math.Min(totalHeight, availableHeight));
    }

    public void Arrange(Rectangle bounds)
    {
        if (_children.Count == 0)
            return;

        int currentX = bounds.X;
        int currentY = bounds.Y;

        if (Orientation == StackOrientation.Vertical)
        {
            foreach (var child in _children)
            {
                if (!child.Visible)
                    continue;

                var childBounds = new Rectangle(
                    currentX,
                    currentY,
                    Math.Min(child.Bounds.Width, bounds.Width),
                    child.Bounds.Height
                );

                child.Bounds = childBounds;
                currentY += child.Bounds.Height + Spacing;

                // Stop if we've exceeded the bounds
                if (currentY >= bounds.Y + bounds.Height)
                    break;
            }
        }
        else // Horizontal
        {
            foreach (var child in _children)
            {
                if (!child.Visible)
                    continue;

                var childBounds = new Rectangle(
                    currentX,
                    currentY,
                    child.Bounds.Width,
                    Math.Min(child.Bounds.Height, bounds.Height)
                );

                child.Bounds = childBounds;
                currentX += child.Bounds.Width + Spacing;

                // Stop if we've exceeded the bounds
                if (currentX >= bounds.X + bounds.Width)
                    break;
            }
        }
    }
}
