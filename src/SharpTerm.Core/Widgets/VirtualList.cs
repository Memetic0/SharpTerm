using SharpTerm.Core.Performance;

namespace SharpTerm.Core.Widgets;

/// <summary>
/// A high-performance list widget optimized for large datasets using virtual rendering.
/// Only visible items are rendered, supporting millions of items efficiently.
/// </summary>
public class VirtualList : Widget
{
    private readonly List<string> _items = new();
    private int _selectedIndex = 0;
    private int _scrollOffset = 0;
    private int _lastClickIndex = -1;
    private DateTime _lastClickTime = DateTime.MinValue;
    private const int DoubleClickThresholdMs = 500;

    // Cache for rendered strings
    private readonly Dictionary<int, string> _renderCache = new();
    private int _lastRenderWidth = -1;

    public IReadOnlyList<string> Items => _items.AsReadOnly();
    public int SelectedIndex => _selectedIndex;
    public bool IsFocused { get; set; }
    public bool ShowScrollbar { get; set; } = true;
    public Color SelectedColor { get; set; } = Color.Blue;
    public Color AlternateRowColor { get; set; } = Color.DarkGray;
    public Color ScrollbarColor { get; set; } = Color.DarkGray;

    public event EventHandler? SelectionChanged;
    public event EventHandler<int>? ItemActivated;

    public void AddItem(string item)
    {
        _items.Add(item);
        InvalidateCache();
        OnChanged();
    }

    public void SetItems(IEnumerable<string> items)
    {
        int previousCount = _items.Count;
        _items.Clear();
        _items.AddRange(items);

        if (_items.Count > previousCount && previousCount > 0)
        {
            _selectedIndex = _items.Count - 1;
        }
        else if (_selectedIndex >= _items.Count)
        {
            _selectedIndex = Math.Max(0, _items.Count - 1);
        }

        AdjustScrollOffset();
        InvalidateCache();
        OnChanged();
    }

    public void Clear()
    {
        _items.Clear();
        _selectedIndex = 0;
        _scrollOffset = 0;
        InvalidateCache();
        OnChanged();
    }

    private void InvalidateCache()
    {
        _renderCache.Clear();
        _lastRenderWidth = -1;
    }

    public override void Render(ITerminalDriver driver)
    {
        if (!Visible || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        int displayHeight = Bounds.Height;
        int displayWidth = ShowScrollbar && _items.Count > displayHeight
            ? Bounds.Width - 1
            : Bounds.Width;

        if (displayWidth <= 0)
            return;

        // Invalidate cache if width changed
        if (_lastRenderWidth != displayWidth)
        {
            InvalidateCache();
            _lastRenderWidth = displayWidth;
        }

        // VIRTUAL RENDERING: Only render visible items
        int endIndex = Math.Min(_scrollOffset + displayHeight, _items.Count);

        for (int i = 0; i < displayHeight; i++)
        {
            int itemIndex = _scrollOffset + i;

            driver.SetCursorPosition(Bounds.X, Bounds.Y + i);

            if (itemIndex >= _items.Count)
            {
                // Fill empty lines
                driver.Write(SpanHelpers.Repeat(' ', displayWidth), ForegroundColor, BackgroundColor);
                continue;
            }

            // Get or create cached display text
            string displayText = GetCachedDisplayText(itemIndex, displayWidth);

            // Determine colors
            Color bg, fg;
            if (itemIndex == _selectedIndex && IsFocused)
            {
                bg = SelectedColor;
                fg = Color.White;
            }
            else if (itemIndex % 2 == 1)
            {
                bg = AlternateRowColor;
                fg = ForegroundColor;
            }
            else
            {
                bg = BackgroundColor;
                fg = ForegroundColor;
            }

            driver.Write(displayText, fg, bg);
        }

        // Render scrollbar if needed
        if (ShowScrollbar && _items.Count > displayHeight)
        {
            RenderScrollbar(driver, displayHeight);
        }
    }

    private string GetCachedDisplayText(int itemIndex, int width)
    {
        // Check cache
        if (_renderCache.TryGetValue(itemIndex, out var cached))
        {
            return cached;
        }

        // Generate display text using Span for efficiency
        var item = _items[itemIndex];
        string displayText;

        if (item.Length > width)
        {
            displayText = SpanHelpers.Truncate(item.AsSpan(), width);
        }
        else
        {
            displayText = SpanHelpers.PadRight(item.AsSpan(), width);
        }

        // Cache only if reasonable cache size
        if (_renderCache.Count < 1000)
        {
            _renderCache[itemIndex] = displayText;
        }

        return displayText;
    }

    private void RenderScrollbar(ITerminalDriver driver, int displayHeight)
    {
        int scrollbarX = Bounds.X + Bounds.Width - 1;

        float viewportRatio = (float)displayHeight / _items.Count;
        int thumbSize = Math.Max(1, (int)(displayHeight * viewportRatio));
        float scrollRatio = _items.Count > displayHeight
            ? (float)_scrollOffset / (_items.Count - displayHeight)
            : 0;
        int thumbPosition = (int)((displayHeight - thumbSize) * scrollRatio);

        for (int i = 0; i < displayHeight; i++)
        {
            driver.SetCursorPosition(scrollbarX, Bounds.Y + i);

            char scrollChar = (i >= thumbPosition && i < thumbPosition + thumbSize)
                ? '█'
                : '│';

            driver.Write(scrollChar.ToString(), ScrollbarColor, BackgroundColor);
        }
    }

    public override bool HandleKey(ConsoleKeyInfo key)
    {
        if (!IsFocused || _items.Count == 0)
            return false;

        bool handled = false;
        int oldIndex = _selectedIndex;

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                if (_selectedIndex > 0)
                {
                    _selectedIndex--;
                    handled = true;
                }
                break;

            case ConsoleKey.DownArrow:
                if (_selectedIndex < _items.Count - 1)
                {
                    _selectedIndex++;
                    handled = true;
                }
                break;

            case ConsoleKey.Home:
                _selectedIndex = 0;
                _scrollOffset = 0;
                handled = true;
                break;

            case ConsoleKey.End:
                _selectedIndex = _items.Count - 1;
                handled = true;
                break;

            case ConsoleKey.PageUp:
                _selectedIndex = Math.Max(0, _selectedIndex - Bounds.Height);
                handled = true;
                break;

            case ConsoleKey.PageDown:
                _selectedIndex = Math.Min(_items.Count - 1, _selectedIndex + Bounds.Height);
                handled = true;
                break;

            case ConsoleKey.Enter:
                ItemActivated?.Invoke(this, _selectedIndex);
                return true;
        }

        if (handled && oldIndex != _selectedIndex)
        {
            AdjustScrollOffset();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            OnChanged();
        }

        return handled;
    }

    private void AdjustScrollOffset()
    {
        if (_selectedIndex < _scrollOffset)
        {
            _scrollOffset = _selectedIndex;
        }
        else if (_selectedIndex >= _scrollOffset + Bounds.Height)
        {
            _scrollOffset = _selectedIndex - Bounds.Height + 1;
        }
    }

    internal void HandleClick(int relativeY)
    {
        int clickedIndex = _scrollOffset + relativeY;
        if (clickedIndex >= 0 && clickedIndex < _items.Count)
        {
            var now = DateTime.Now;
            bool isDoubleClick = clickedIndex == _lastClickIndex
                && (now - _lastClickTime).TotalMilliseconds < DoubleClickThresholdMs;

            _selectedIndex = clickedIndex;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            OnChanged();

            if (isDoubleClick)
            {
                ItemActivated?.Invoke(this, _selectedIndex);
                _lastClickIndex = -1;
            }
            else
            {
                _lastClickIndex = clickedIndex;
                _lastClickTime = now;
            }
        }
    }

    internal void HandleScroll(int scrollDelta)
    {
        int newIndex = _selectedIndex - scrollDelta;
        newIndex = Math.Clamp(newIndex, 0, _items.Count - 1);

        if (newIndex != _selectedIndex)
        {
            _selectedIndex = newIndex;
            AdjustScrollOffset();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            OnChanged();
        }
    }
}
