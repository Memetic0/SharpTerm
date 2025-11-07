namespace SharpTerm.Core.Widgets;

/// <summary>
/// A scrollable list widget that displays items with selection and keyboard navigation.
/// </summary>
public class List : Widget
{
    private readonly List<string> _items = new();
    private int _selectedIndex = 0;
    private int _scrollOffset = 0;
    private int _lastClickIndex = -1;
    private DateTime _lastClickTime = DateTime.MinValue;
    private const int DoubleClickThresholdMs = 500;
    
    public IReadOnlyList<string> Items => _items.AsReadOnly();
    public int SelectedIndex => _selectedIndex;
    public bool IsFocused { get; set; }
    public bool ShowScrollbar { get; set; } = true;
    public Color SelectedColor { get; set; } = Color.Blue;
    public Color ScrollbarColor { get; set; } = Color.DarkGray;
    
    /// <summary>
    /// Event raised when the selected item changes.
    /// </summary>
    public event EventHandler? SelectionChanged;
    
    /// <summary>
    /// Event raised when an item is activated (Enter key or double-click).
    /// </summary>
    public event EventHandler<int>? ItemActivated;
    
    public void AddItem(string item)
    {
        _items.Add(item);
        OnChanged();
    }
    
    public void RemoveItem(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            _items.RemoveAt(index);
            if (_selectedIndex >= _items.Count)
            {
                _selectedIndex = Math.Max(0, _items.Count - 1);
            }
            AdjustScrollOffset();
            OnChanged();
        }
    }
    
    public void Clear()
    {
        _items.Clear();
        _selectedIndex = 0;
        _scrollOffset = 0;
        OnChanged();
    }
    
    public void SetItems(IEnumerable<string> items)
    {
        int previousCount = _items.Count;
        _items.Clear();
        _items.AddRange(items);
        
        // If items were added, select the last one (the new item)
        if (_items.Count > previousCount && previousCount > 0)
        {
            _selectedIndex = _items.Count - 1;
        }
        else if (_selectedIndex >= _items.Count)
        {
            _selectedIndex = Math.Max(0, _items.Count - 1);
        }
        
        AdjustScrollOffset();
        OnChanged();
    }
    
    public override void Render(ITerminalDriver driver)
    {
        if (!Visible) return;
        
        int displayHeight = Bounds.Height;
        int displayWidth = ShowScrollbar && _items.Count > displayHeight ? Bounds.Width - 1 : Bounds.Width;
        
        // Render visible items
        for (int i = 0; i < displayHeight && i < _items.Count; i++)
        {
            int itemIndex = _scrollOffset + i;
            if (itemIndex >= _items.Count) break;
            
            driver.SetCursorPosition(Bounds.X, Bounds.Y + i);
            
            var item = _items[itemIndex];
            var displayText = item.Length > displayWidth 
                ? item.Substring(0, displayWidth) 
                : item.PadRight(displayWidth);
            
            var bg = (itemIndex == _selectedIndex && IsFocused) ? SelectedColor : BackgroundColor;
            var fg = (itemIndex == _selectedIndex && IsFocused) ? Color.White : ForegroundColor;
            
            driver.Write(displayText, fg, bg);
        }
        
        // Fill empty lines
        for (int i = _items.Count - _scrollOffset; i < displayHeight; i++)
        {
            if (i < 0) continue;
            driver.SetCursorPosition(Bounds.X, Bounds.Y + i);
            driver.Write(new string(' ', displayWidth), ForegroundColor, BackgroundColor);
        }
        
        // Render scrollbar if needed
        if (ShowScrollbar && _items.Count > displayHeight)
        {
            RenderScrollbar(driver, displayHeight);
        }
    }
    
    private void RenderScrollbar(ITerminalDriver driver, int displayHeight)
    {
        int scrollbarX = Bounds.X + Bounds.Width - 1;
        
        // Calculate scrollbar thumb position and size
        float viewportRatio = (float)displayHeight / _items.Count;
        int thumbSize = Math.Max(1, (int)(displayHeight * viewportRatio));
        float scrollRatio = _items.Count > displayHeight ? (float)_scrollOffset / (_items.Count - displayHeight) : 0;
        int thumbPosition = (int)((displayHeight - thumbSize) * scrollRatio);
        
        for (int i = 0; i < displayHeight; i++)
        {
            driver.SetCursorPosition(scrollbarX, Bounds.Y + i);
            
            char scrollChar;
            if (i >= thumbPosition && i < thumbPosition + thumbSize)
            {
                scrollChar = '█'; // Thumb
            }
            else
            {
                scrollChar = '│'; // Track
            }
            
            driver.Write(scrollChar.ToString(), ScrollbarColor, BackgroundColor);
        }
    }
    
    public override bool HandleKey(ConsoleKeyInfo key)
    {
        if (!IsFocused || _items.Count == 0) return false;
        
        bool handled = false;
        
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                if (_selectedIndex > 0)
                {
                    _selectedIndex--;
                    AdjustScrollOffset();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    OnChanged();
                    handled = true;
                }
                break;
                
            case ConsoleKey.DownArrow:
                if (_selectedIndex < _items.Count - 1)
                {
                    _selectedIndex++;
                    AdjustScrollOffset();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    OnChanged();
                    handled = true;
                }
                break;
                
            case ConsoleKey.Home:
                if (_selectedIndex != 0)
                {
                    _selectedIndex = 0;
                    _scrollOffset = 0;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    OnChanged();
                    handled = true;
                }
                break;
                
            case ConsoleKey.End:
                if (_selectedIndex != _items.Count - 1)
                {
                    _selectedIndex = _items.Count - 1;
                    AdjustScrollOffset();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    OnChanged();
                    handled = true;
                }
                break;
                
            case ConsoleKey.PageUp:
                if (_selectedIndex > 0)
                {
                    _selectedIndex = Math.Max(0, _selectedIndex - Bounds.Height);
                    AdjustScrollOffset();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    OnChanged();
                    handled = true;
                }
                break;
                
            case ConsoleKey.PageDown:
                if (_selectedIndex < _items.Count - 1)
                {
                    _selectedIndex = Math.Min(_items.Count - 1, _selectedIndex + Bounds.Height);
                    AdjustScrollOffset();
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    OnChanged();
                    handled = true;
                }
                break;
                
            case ConsoleKey.Enter:
                if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                {
                    ItemActivated?.Invoke(this, _selectedIndex);
                    handled = true;
                }
                break;
        }
        
        return handled;
    }
    
    private void AdjustScrollOffset()
    {
        // Keep selected item visible
        if (_selectedIndex < _scrollOffset)
        {
            _scrollOffset = _selectedIndex;
        }
        else if (_selectedIndex >= _scrollOffset + Bounds.Height)
        {
            _scrollOffset = _selectedIndex - Bounds.Height + 1;
        }
    }
    
    /// <summary>
    /// Handles mouse click on the list.
    /// </summary>
    internal void HandleClick(int relativeY)
    {
        int clickedIndex = _scrollOffset + relativeY;
        if (clickedIndex >= 0 && clickedIndex < _items.Count)
        {
            var now = DateTime.Now;
            bool isDoubleClick = clickedIndex == _lastClickIndex && 
                                (now - _lastClickTime).TotalMilliseconds < DoubleClickThresholdMs;
            
            _selectedIndex = clickedIndex;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            OnChanged();
            
            if (isDoubleClick)
            {
                ItemActivated?.Invoke(this, _selectedIndex);
                _lastClickIndex = -1; // Reset to prevent triple-click
            }
            else
            {
                _lastClickIndex = clickedIndex;
                _lastClickTime = now;
            }
        }
    }
    
    /// <summary>
    /// Handles mouse scroll on the list.
    /// </summary>
    internal void HandleScroll(int scrollDelta)
    {
        // Positive delta = scroll up (move selection up)
        // Negative delta = scroll down (move selection down)
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
