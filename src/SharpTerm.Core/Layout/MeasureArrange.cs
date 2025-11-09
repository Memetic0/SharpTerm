namespace SharpTerm.Core.Layout;

/// <summary>
/// Represents size constraints for widget measurement.
/// </summary>
public readonly struct Size
{
    public int Width { get; init; }
    public int Height { get; init; }

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static Size Infinite => new Size(int.MaxValue, int.MaxValue);
    public static Size Zero => new Size(0, 0);

    public bool IsInfinite => Width == int.MaxValue || Height == int.MaxValue;

    public Size Constrain(Size constraint)
    {
        return new Size(
            Math.Min(Width, constraint.Width),
            Math.Min(Height, constraint.Height)
        );
    }

    public override string ToString() => $"({Width}, {Height})";
}

/// <summary>
/// Represents thickness for margins and padding.
/// </summary>
public readonly struct Thickness
{
    public int Left { get; init; }
    public int Top { get; init; }
    public int Right { get; init; }
    public int Bottom { get; init; }

    public Thickness(int uniform)
    {
        Left = Top = Right = Bottom = uniform;
    }

    public Thickness(int horizontal, int vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }

    public Thickness(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;

    public static Thickness Zero => new Thickness(0);

    public override string ToString() => $"({Left}, {Top}, {Right}, {Bottom})";
}

/// <summary>
/// Interface for widgets that support measure/arrange layout.
/// </summary>
public interface IMeasurable
{
    /// <summary>
    /// Measures the desired size of the widget given the available size.
    /// </summary>
    Size Measure(Size availableSize);

    /// <summary>
    /// Arranges the widget within the specified bounds.
    /// </summary>
    void Arrange(Rectangle finalRect);

    /// <summary>
    /// Gets the desired size calculated during measure pass.
    /// </summary>
    Size DesiredSize { get; }
}

/// <summary>
/// Base class for widgets that support measure/arrange layout.
/// </summary>
public abstract class MeasurableWidget : Widget, IMeasurable
{
    private Size _desiredSize;
    private bool _measureDirty = true;
    private bool _arrangeDirty = true;

    /// <summary>
    /// Gets or sets the margin around the widget.
    /// </summary>
    public Thickness Margin { get; set; } = Thickness.Zero;

    /// <summary>
    /// Gets or sets the padding inside the widget.
    /// </summary>
    public Thickness Padding { get; set; } = Thickness.Zero;

    /// <summary>
    /// Gets or sets the minimum width.
    /// </summary>
    public int? MinWidth { get; set; }

    /// <summary>
    /// Gets or sets the minimum height.
    /// </summary>
    public int? MinHeight { get; set; }

    /// <summary>
    /// Gets or sets the maximum width.
    /// </summary>
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum height.
    /// </summary>
    public int? MaxHeight { get; set; }

    /// <summary>
    /// Gets or sets the desired width (null for auto).
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the desired height (null for auto).
    /// </summary>
    public int? Height { get; set; }

    public Size DesiredSize => _desiredSize;

    /// <summary>
    /// Measures the widget.
    /// </summary>
    public Size Measure(Size availableSize)
    {
        // Apply margin
        var marginSize = new Size(
            Math.Max(0, availableSize.Width - Margin.Horizontal),
            Math.Max(0, availableSize.Height - Margin.Vertical)
        );

        // Apply explicit size constraints
        var constrainedSize = ApplySizeConstraints(marginSize);

        // Measure core
        var desiredSize = MeasureCore(constrainedSize);

        // Apply size constraints to desired size
        desiredSize = ApplySizeConstraints(desiredSize);

        // Add margin back
        _desiredSize = new Size(
            desiredSize.Width + Margin.Horizontal,
            desiredSize.Height + Margin.Vertical
        );

        _measureDirty = false;
        return _desiredSize;
    }

    /// <summary>
    /// Arranges the widget.
    /// </summary>
    public void Arrange(Rectangle finalRect)
    {
        // Remove margin
        var arrangeRect = new Rectangle(
            finalRect.X + Margin.Left,
            finalRect.Y + Margin.Top,
            Math.Max(0, finalRect.Width - Margin.Horizontal),
            Math.Max(0, finalRect.Height - Margin.Vertical)
        );

        // Arrange core
        ArrangeCore(arrangeRect);

        // Update bounds
        Bounds = finalRect;

        _arrangeDirty = false;
        OnChanged();
    }

    /// <summary>
    /// Invalidates the measure pass.
    /// </summary>
    public void InvalidateMeasure()
    {
        _measureDirty = true;
        (Parent as MeasurableWidget)?.InvalidateMeasure();
    }

    /// <summary>
    /// Invalidates the arrange pass.
    /// </summary>
    public void InvalidateArrange()
    {
        _arrangeDirty = true;
        (Parent as MeasurableWidget)?.InvalidateArrange();
    }

    /// <summary>
    /// Core measure logic to be implemented by derived classes.
    /// </summary>
    protected virtual Size MeasureCore(Size availableSize)
    {
        // Default implementation: measure children and return max size
        var maxWidth = 0;
        var maxHeight = 0;

        foreach (var child in Children.OfType<IMeasurable>())
        {
            var childSize = child.Measure(availableSize);
            maxWidth = Math.Max(maxWidth, childSize.Width);
            maxHeight = Math.Max(maxHeight, childSize.Height);
        }

        return new Size(maxWidth, maxHeight);
    }

    /// <summary>
    /// Core arrange logic to be implemented by derived classes.
    /// </summary>
    protected virtual void ArrangeCore(Rectangle finalRect)
    {
        // Default implementation: arrange each child to fill the entire rect
        foreach (var child in Children.OfType<IMeasurable>())
        {
            child.Arrange(finalRect);
        }
    }

    private Size ApplySizeConstraints(Size size)
    {
        var width = Width ?? size.Width;
        var height = Height ?? size.Height;

        // Apply min/max constraints
        if (MinWidth.HasValue) width = Math.Max(width, MinWidth.Value);
        if (MinHeight.HasValue) height = Math.Max(height, MinHeight.Value);
        if (MaxWidth.HasValue) width = Math.Min(width, MaxWidth.Value);
        if (MaxHeight.HasValue) height = Math.Min(height, MaxHeight.Value);

        return new Size(width, height);
    }
}

/// <summary>
/// Helper for performing layout passes on a widget tree.
/// </summary>
public static class LayoutHelper
{
    /// <summary>
    /// Performs a complete measure and arrange pass on a widget tree.
    /// </summary>
    public static void UpdateLayout(Widget root, Size availableSize)
    {
        if (root is IMeasurable measurable)
        {
            // Measure pass
            measurable.Measure(availableSize);

            // Arrange pass
            var arrangeRect = new Rectangle(0, 0, availableSize.Width, availableSize.Height);
            measurable.Arrange(arrangeRect);
        }
    }

    /// <summary>
    /// Performs a measure pass on a widget tree.
    /// </summary>
    public static Size MeasureWidget(Widget widget, Size availableSize)
    {
        if (widget is IMeasurable measurable)
        {
            return measurable.Measure(availableSize);
        }

        return new Size(widget.Bounds.Width, widget.Bounds.Height);
    }

    /// <summary>
    /// Performs an arrange pass on a widget tree.
    /// </summary>
    public static void ArrangeWidget(Widget widget, Rectangle finalRect)
    {
        if (widget is IMeasurable measurable)
        {
            measurable.Arrange(finalRect);
        }
        else
        {
            widget.Bounds = finalRect;
        }
    }
}
