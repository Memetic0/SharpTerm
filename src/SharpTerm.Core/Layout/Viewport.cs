namespace SharpTerm.Core.Layout;

/// <summary>
/// Represents a scrollable viewport for content larger than the visible area.
/// </summary>
public class Viewport
{
    private Rectangle _contentBounds;
    private Rectangle _visibleBounds;
    private int _scrollOffsetX;
    private int _scrollOffsetY;

    /// <summary>
    /// Gets or sets the bounds of the entire content.
    /// </summary>
    public Rectangle ContentBounds
    {
        get => _contentBounds;
        set
        {
            _contentBounds = value;
            ClampScrollOffsets();
        }
    }

    /// <summary>
    /// Gets or sets the visible portion of the viewport.
    /// </summary>
    public Rectangle VisibleBounds
    {
        get => _visibleBounds;
        set
        {
            _visibleBounds = value;
            ClampScrollOffsets();
        }
    }

    /// <summary>
    /// Gets or sets the horizontal scroll offset.
    /// </summary>
    public int ScrollOffsetX
    {
        get => _scrollOffsetX;
        set
        {
            _scrollOffsetX = value;
            ClampScrollOffsets();
        }
    }

    /// <summary>
    /// Gets or sets the vertical scroll offset.
    /// </summary>
    public int ScrollOffsetY
    {
        get => _scrollOffsetY;
        set
        {
            _scrollOffsetY = value;
            ClampScrollOffsets();
        }
    }

    /// <summary>
    /// Gets the visible content rectangle (content coordinates).
    /// </summary>
    public Rectangle VisibleContentRect => new Rectangle(
        _scrollOffsetX,
        _scrollOffsetY,
        _visibleBounds.Width,
        _visibleBounds.Height
    );

    /// <summary>
    /// Gets whether horizontal scrolling is needed.
    /// </summary>
    public bool NeedsHorizontalScroll => _contentBounds.Width > _visibleBounds.Width;

    /// <summary>
    /// Gets whether vertical scrolling is needed.
    /// </summary>
    public bool NeedsVerticalScroll => _contentBounds.Height > _visibleBounds.Height;

    /// <summary>
    /// Gets the maximum horizontal scroll offset.
    /// </summary>
    public int MaxScrollX => Math.Max(0, _contentBounds.Width - _visibleBounds.Width);

    /// <summary>
    /// Gets the maximum vertical scroll offset.
    /// </summary>
    public int MaxScrollY => Math.Max(0, _contentBounds.Height - _visibleBounds.Height);

    /// <summary>
    /// Scrolls by the specified delta.
    /// </summary>
    public void Scroll(int deltaX, int deltaY)
    {
        ScrollOffsetX += deltaX;
        ScrollOffsetY += deltaY;
    }

    /// <summary>
    /// Scrolls to make the specified rectangle visible.
    /// </summary>
    public void ScrollIntoView(Rectangle rect)
    {
        // Scroll horizontally
        if (rect.X < _scrollOffsetX)
        {
            _scrollOffsetX = rect.X;
        }
        else if (rect.X + rect.Width > _scrollOffsetX + _visibleBounds.Width)
        {
            _scrollOffsetX = rect.X + rect.Width - _visibleBounds.Width;
        }

        // Scroll vertically
        if (rect.Y < _scrollOffsetY)
        {
            _scrollOffsetY = rect.Y;
        }
        else if (rect.Y + rect.Height > _scrollOffsetY + _visibleBounds.Height)
        {
            _scrollOffsetY = rect.Y + rect.Height - _visibleBounds.Height;
        }

        ClampScrollOffsets();
    }

    /// <summary>
    /// Converts content coordinates to viewport coordinates.
    /// </summary>
    public Rectangle ContentToViewport(Rectangle contentRect)
    {
        return new Rectangle(
            contentRect.X - _scrollOffsetX + _visibleBounds.X,
            contentRect.Y - _scrollOffsetY + _visibleBounds.Y,
            contentRect.Width,
            contentRect.Height
        );
    }

    /// <summary>
    /// Converts viewport coordinates to content coordinates.
    /// </summary>
    public Rectangle ViewportToContent(Rectangle viewportRect)
    {
        return new Rectangle(
            viewportRect.X - _visibleBounds.X + _scrollOffsetX,
            viewportRect.Y - _visibleBounds.Y + _scrollOffsetY,
            viewportRect.Width,
            viewportRect.Height
        );
    }

    /// <summary>
    /// Determines if a content rectangle is visible in the viewport.
    /// </summary>
    public bool IsVisible(Rectangle contentRect)
    {
        var visibleContent = VisibleContentRect;
        return contentRect.IntersectsWith(visibleContent);
    }

    /// <summary>
    /// Clips a content rectangle to the visible portion.
    /// </summary>
    public Rectangle? Clip(Rectangle contentRect)
    {
        var visibleContent = VisibleContentRect;
        if (!contentRect.IntersectsWith(visibleContent))
            return null;

        var x = Math.Max(contentRect.X, visibleContent.X);
        var y = Math.Max(contentRect.Y, visibleContent.Y);
        var right = Math.Min(contentRect.X + contentRect.Width, visibleContent.X + visibleContent.Width);
        var bottom = Math.Min(contentRect.Y + contentRect.Height, visibleContent.Y + visibleContent.Height);

        return new Rectangle(x, y, right - x, bottom - y);
    }

    private void ClampScrollOffsets()
    {
        _scrollOffsetX = Math.Clamp(_scrollOffsetX, 0, MaxScrollX);
        _scrollOffsetY = Math.Clamp(_scrollOffsetY, 0, MaxScrollY);
    }
}

/// <summary>
/// Widget that provides viewport/scrolling functionality.
/// </summary>
public class ScrollableWidget : Widget
{
    private readonly Viewport _viewport = new();
    private Widget? _content;

    public Viewport Viewport => _viewport;

    /// <summary>
    /// Gets or sets the content widget.
    /// </summary>
    public Widget? Content
    {
        get => _content;
        set
        {
            if (_content != null)
            {
                RemoveChild(_content);
            }

            _content = value;

            if (_content != null)
            {
                AddChild(_content);
                _viewport.ContentBounds = _content.Bounds;
            }

            OnChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether horizontal scrolling is enabled.
    /// </summary>
    public bool HorizontalScrollEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether vertical scrolling is enabled.
    /// </summary>
    public bool VerticalScrollEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show scroll indicators.
    /// </summary>
    public bool ShowScrollIndicators { get; set; } = true;

    public override void Render(ITerminalDriver driver)
    {
        if (!Visible || _content == null)
            return;

        _viewport.VisibleBounds = Bounds;

        // Save current clip region (if driver supports it)
        // For now, we'll manually handle clipping

        // Only render content that's visible
        if (_viewport.IsVisible(_content.Bounds))
        {
            // Translate content position based on scroll offset
            var originalBounds = _content.Bounds;
            var viewportBounds = _viewport.ContentToViewport(_content.Bounds);

            // Temporarily adjust content bounds for rendering
            _content.Bounds = viewportBounds;
            _content.Render(driver);
            _content.Bounds = originalBounds;
        }

        // Draw scroll indicators
        if (ShowScrollIndicators)
        {
            DrawScrollIndicators(driver);
        }
    }

    private void DrawScrollIndicators(ITerminalDriver driver)
    {
        // Draw vertical scrollbar
        if (VerticalScrollEnabled && _viewport.NeedsVerticalScroll)
        {
            var scrollbarX = Bounds.X + Bounds.Width - 1;
            var scrollbarHeight = Bounds.Height;
            var thumbHeight = Math.Max(1, scrollbarHeight * Bounds.Height / _viewport.ContentBounds.Height);
            var thumbPosition = Bounds.Y + (_viewport.ScrollOffsetY * scrollbarHeight / _viewport.ContentBounds.Height);

            for (int y = Bounds.Y; y < Bounds.Y + Bounds.Height; y++)
            {
                driver.SetCursorPosition(scrollbarX, y);
                if (y >= thumbPosition && y < thumbPosition + thumbHeight)
                {
                    driver.Write("█", ForegroundColor, BackgroundColor);
                }
                else
                {
                    driver.Write("│", Color.DarkGray, BackgroundColor);
                }
            }
        }

        // Draw horizontal scrollbar
        if (HorizontalScrollEnabled && _viewport.NeedsHorizontalScroll)
        {
            var scrollbarY = Bounds.Y + Bounds.Height - 1;
            var scrollbarWidth = Bounds.Width;
            var thumbWidth = Math.Max(1, scrollbarWidth * Bounds.Width / _viewport.ContentBounds.Width);
            var thumbPosition = Bounds.X + (_viewport.ScrollOffsetX * scrollbarWidth / _viewport.ContentBounds.Width);

            for (int x = Bounds.X; x < Bounds.X + Bounds.Width; x++)
            {
                driver.SetCursorPosition(x, scrollbarY);
                if (x >= thumbPosition && x < thumbPosition + thumbWidth)
                {
                    driver.Write("█", ForegroundColor, BackgroundColor);
                }
                else
                {
                    driver.Write("─", Color.DarkGray, BackgroundColor);
                }
            }
        }
    }

    public override bool HandleKey(ConsoleKeyInfo key)
    {
        // Handle scroll keys
        switch (key.Key)
        {
            case ConsoleKey.UpArrow when VerticalScrollEnabled:
                _viewport.Scroll(0, -1);
                OnChanged();
                return true;

            case ConsoleKey.DownArrow when VerticalScrollEnabled:
                _viewport.Scroll(0, 1);
                OnChanged();
                return true;

            case ConsoleKey.LeftArrow when HorizontalScrollEnabled:
                _viewport.Scroll(-1, 0);
                OnChanged();
                return true;

            case ConsoleKey.RightArrow when HorizontalScrollEnabled:
                _viewport.Scroll(1, 0);
                OnChanged();
                return true;

            case ConsoleKey.PageUp when VerticalScrollEnabled:
                _viewport.Scroll(0, -Bounds.Height);
                OnChanged();
                return true;

            case ConsoleKey.PageDown when VerticalScrollEnabled:
                _viewport.Scroll(0, Bounds.Height);
                OnChanged();
                return true;

            case ConsoleKey.Home when VerticalScrollEnabled:
                _viewport.ScrollOffsetY = 0;
                OnChanged();
                return true;

            case ConsoleKey.End when VerticalScrollEnabled:
                _viewport.ScrollOffsetY = _viewport.MaxScrollY;
                OnChanged();
                return true;
        }

        return _content?.HandleKey(key) ?? false;
    }
}
