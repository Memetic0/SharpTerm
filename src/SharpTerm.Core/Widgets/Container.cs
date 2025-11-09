using SharpTerm.Core.Layout;

namespace SharpTerm.Core.Widgets;

/// <summary>
/// A container widget that can hold child widgets and apply a layout manager.
/// </summary>
public class Container : Widget
{
    private ILayoutManager? _layoutManager;

    /// <summary>
    /// Gets or sets the layout manager for this container.
    /// </summary>
    public ILayoutManager? LayoutManager
    {
        get => _layoutManager;
        set
        {
            _layoutManager = value;
            if (_layoutManager != null)
            {
                // Transfer existing children to the layout manager
                foreach (var child in Children)
                {
                    _layoutManager.AddChild(child);
                }
                ArrangeLayout();
            }
            OnChanged();
        }
    }

    /// <summary>
    /// Padding inside the container.
    /// </summary>
    public int Padding { get; set; } = 0;

    public override void AddChild(Widget child)
    {
        base.AddChild(child);
        _layoutManager?.AddChild(child);
        ArrangeLayout();
    }

    public override void RemoveChild(Widget child)
    {
        base.RemoveChild(child);
        _layoutManager?.RemoveChild(child);
        ArrangeLayout();
    }

    /// <summary>
    /// Arranges children using the layout manager.
    /// </summary>
    public void ArrangeLayout()
    {
        if (_layoutManager == null || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        // Calculate available space accounting for padding
        var layoutBounds = new Rectangle(
            Bounds.X + Padding,
            Bounds.Y + Padding,
            Math.Max(0, Bounds.Width - Padding * 2),
            Math.Max(0, Bounds.Height - Padding * 2)
        );

        _layoutManager.Arrange(layoutBounds);
    }

    public override void Render(ITerminalDriver driver)
    {
        if (!Visible)
            return;

        // Don't render if bounds are invalid
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        // Fill background if not transparent
        if (BackgroundColor.R != 0 || BackgroundColor.G != 0 || BackgroundColor.B != 1)
        {
            for (int y = 0; y < Bounds.Height; y++)
            {
                driver.SetCursorPosition(Bounds.X, Bounds.Y + y);
                driver.Write(new string(' ', Bounds.Width), ForegroundColor, BackgroundColor);
            }
        }

        // Render all children recursively
        foreach (var child in Children)
        {
            if (child.Visible)
            {
                child.RenderTree(driver);
            }
        }
    }
}
