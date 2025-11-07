namespace SharpTerm.Core;

/// <summary>
/// Main application class that manages the render loop and input handling.
/// </summary>
public class Application
{
    private readonly ITerminalDriver _driver;
    private readonly List<Widget> _widgets = new();
    private readonly HashSet<Widget> _dirtyWidgets = new();
    private bool _running;
    private int _focusedIndex = -1;
    private bool _needsFullRedraw = true; // Track if full screen redraw needed
    private bool _debugMode;
    private StreamWriter? _debugLogWriter;
    private int _lastWidth;
    private int _lastHeight;
    
    /// <summary>
    /// Event raised when the terminal is resized.
    /// </summary>
    public event EventHandler? Resized;

    public Application(ITerminalDriver? driver = null, bool debugMode = false)
    {
        _driver = driver ?? new AnsiTerminalDriver();
        _debugMode = debugMode;
        _lastWidth = Console.WindowWidth;
        _lastHeight = Console.WindowHeight;

        if (_debugMode)
        {
            try
            {
                var logPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "sharpterm-debug.log"
                );
                // Overwrite existing log file
                _debugLogWriter = new StreamWriter(logPath, false) { AutoFlush = true };
                _debugLogWriter.WriteLine(
                    $"=== SharpTerm Debug Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==="
                );
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }

    private void DebugLog(string message)
    {
        if (_debugMode && _debugLogWriter != null)
        {
            try
            {
                _debugLogWriter.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }

    private void InitializeFocus()
    {
        // Set initial focus to first focusable widget
        var firstFocusable = _widgets
            .Select((w, i) => new { Widget = w, Index = i })
            .FirstOrDefault(x => x.Widget is Widgets.Button or Widgets.TextBox or Widgets.List);

        if (firstFocusable != null)
        {
            _focusedIndex = firstFocusable.Index;
            if (firstFocusable.Widget is Widgets.Button button)
            {
                button.IsFocused = true;
            }
            else if (firstFocusable.Widget is Widgets.TextBox textBox)
            {
                textBox.IsFocused = true;
            }
        }
    }

    /// <summary>
    /// Adds a widget to the application.
    /// </summary>
    public void AddWidget(Widget widget)
    {
        widget.Changed += (s, e) =>
        {
            if (s is Widget w)
            {
                lock (_dirtyWidgets)
                {
                    _dirtyWidgets.Add(w);
                }
            }
        };
        _widgets.Add(widget);
        _needsFullRedraw = true;
    }

    /// <summary>
    /// Removes a widget from the application.
    /// </summary>
    public void RemoveWidget(Widget widget)
    {
        _widgets.Remove(widget);
        lock (_dirtyWidgets)
        {
            _dirtyWidgets.Remove(widget);
        }
        _needsFullRedraw = true;
    }

    /// <summary>
    /// Requests a full screen redraw on the next frame.
    /// </summary>
    public void RequestRedraw()
    {
        _needsFullRedraw = true;
    }

    /// <summary>
    /// Starts the application main loop.
    /// </summary>
    public void Run()
    {
        _running = true;
        InitializeFocus();

        try
        {
            while (_running)
            {
                // Check for terminal resize
                int currentWidth = Console.WindowWidth;
                int currentHeight = Console.WindowHeight;
                if (currentWidth != _lastWidth || currentHeight != _lastHeight)
                {
                    _lastWidth = currentWidth;
                    _lastHeight = currentHeight;
                    _needsFullRedraw = true;
                    DebugLog($"Terminal resized to {currentWidth}x{currentHeight}");
                    Resized?.Invoke(this, EventArgs.Empty);
                }

                if (_needsFullRedraw)
                {
                    RenderAll();
                    _needsFullRedraw = false;
                    lock (_dirtyWidgets)
                    {
                        _dirtyWidgets.Clear();
                    }
                }
                else
                {
                    RenderDirtyWidgets();
                }

                ProcessInput();
                Thread.Sleep(16); // ~60 FPS
            }
        }
        finally
        {
            _driver.Dispose();

            if (_debugLogWriter != null)
            {
                _debugLogWriter.WriteLine(
                    $"=== SharpTerm Debug Log Ended at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==="
                );
                _debugLogWriter.Dispose();
            }
        }
    }

    /// <summary>
    /// Stops the application.
    /// </summary>
    public void Stop() => _running = false;

    private void RenderAll()
    {
        _driver.Clear();
        foreach (var widget in _widgets)
        {
            widget.Render(_driver);
        }
        _driver.Flush();
    }

    private void RenderDirtyWidgets()
    {
        Widget[] dirtyArray;
        lock (_dirtyWidgets)
        {
            if (_dirtyWidgets.Count == 0)
                return;

            dirtyArray = _dirtyWidgets.ToArray();
            _dirtyWidgets.Clear();
        }

        foreach (var widget in dirtyArray)
        {
            widget.Render(_driver);
        }
        _driver.Flush();
    }

    private void ProcessInput()
    {
        // Process ALL pending input events in the queue
        const int maxEventsPerFrame = 50;
        int eventsProcessed = 0;

        while (eventsProcessed < maxEventsPerFrame && _driver.HasInputEvents)
        {
            var eventType = _driver.ReadConsoleEvent(out var keyInfo, out var mouseEvent);

            if (eventType == ConsoleEventType.None)
            {
                break; // No more events
            }

            eventsProcessed++;

            if (eventType == ConsoleEventType.Keyboard && keyInfo.HasValue)
            {
                var key = keyInfo.Value;
                DebugLog($"Key pressed: {key.Key} (Char='{key.KeyChar}')");

                // ESC has absolute priority - stop immediately
                if (key.Key == ConsoleKey.Escape)
                {
                    DebugLog("ESC pressed - stopping application");
                    Stop();
                    return;
                }

                // Tab navigation
                if (key.Key == ConsoleKey.Tab)
                {
                    DebugLog("TAB pressed - cycling focus");
                    CycleFocus();
                    _needsFullRedraw = true;
                    continue;
                }

                // Pass to focused widget
                if (_focusedIndex >= 0 && _focusedIndex < _widgets.Count)
                {
                    var widget = _widgets[_focusedIndex];
                    bool keyHandled = widget.HandleKey(key);
                    if (keyHandled)
                    {
                        DebugLog($"Key handled by widget at index {_focusedIndex}");
                        lock (_dirtyWidgets)
                        {
                            _dirtyWidgets.Add(widget);
                        }
                    }
                }
            }
            else if (eventType == ConsoleEventType.Mouse && mouseEvent != null)
            {
                if (mouseEvent.ScrollDelta != 0)
                {
                    DebugLog($"Mouse scroll: Delta={mouseEvent.ScrollDelta} at X={mouseEvent.X}, Y={mouseEvent.Y}");
                    HandleMouseScroll(mouseEvent.X, mouseEvent.Y, mouseEvent.ScrollDelta);
                }
                else
                {
                    DebugLog($"Mouse click: X={mouseEvent.X}, Y={mouseEvent.Y}");
                    HandleMouseClick(mouseEvent.X, mouseEvent.Y);
                }
            }
            else if (eventType == ConsoleEventType.Other)
            {
                DebugLog("Other console event consumed (key up, mouse move, focus, resize, etc)");
            }
        }

        if (eventsProcessed > 0)
        {
            DebugLog($"Processed {eventsProcessed} input events this frame");
        }
    }

    private void HandleMouseClick(int x, int y)
    {
        // Check if any clickable widget was clicked
        for (int i = 0; i < _widgets.Count; i++)
        {
            var widget = _widgets[i];
            if (!widget.Visible)
                continue;

            bool inBounds =
                x >= widget.Bounds.X
                && x < widget.Bounds.X + widget.Bounds.Width
                && y >= widget.Bounds.Y
                && y < widget.Bounds.Y + widget.Bounds.Height;

            if (!inBounds)
                continue;

            if (widget is Widgets.Button button)
            {
                DebugLog($"Button clicked at ({x}, {y}) - widget index {i}");
                // Switch focus to this button
                ClearFocus();
                _focusedIndex = i;
                button.IsFocused = true;
                button.InvokeClick();
                _needsFullRedraw = true;
                return;
            }
            else if (widget is Widgets.TextBox textBox)
            {
                DebugLog($"TextBox clicked at ({x}, {y}) - widget index {i}");
                // Switch focus to this textbox
                ClearFocus();
                _focusedIndex = i;
                textBox.IsFocused = true;
                _needsFullRedraw = true;
                return;
            }
            else if (widget is Widgets.List list)
            {
                DebugLog($"List clicked at ({x}, {y}) - widget index {i}");
                // Switch focus to this list
                ClearFocus();
                _focusedIndex = i;
                list.IsFocused = true;
                // Handle the click to select item
                int relativeY = y - widget.Bounds.Y;
                list.HandleClick(relativeY);
                _needsFullRedraw = true;
                return;
            }
        }

        DebugLog($"Click at ({x}, {y}) - no widget hit");
    }
    
    private void HandleMouseScroll(int x, int y, int scrollDelta)
    {
        // Check if scrolling over a List widget
        for (int i = 0; i < _widgets.Count; i++)
        {
            var widget = _widgets[i];
            if (!widget.Visible)
                continue;

            bool inBounds =
                x >= widget.Bounds.X
                && x < widget.Bounds.X + widget.Bounds.Width
                && y >= widget.Bounds.Y
                && y < widget.Bounds.Y + widget.Bounds.Height;

            if (!inBounds)
                continue;

            if (widget is Widgets.List list)
            {
                DebugLog($"List scrolled at ({x}, {y}) - widget index {i}, delta={scrollDelta}");
                list.HandleScroll(scrollDelta);
                _needsFullRedraw = true;
                return;
            }
        }
    }

    private void ClearFocus()
    {
        if (_focusedIndex >= 0 && _focusedIndex < _widgets.Count)
        {
            if (_widgets[_focusedIndex] is Widgets.Button button)
            {
                button.IsFocused = false;
            }
            else if (_widgets[_focusedIndex] is Widgets.TextBox textBox)
            {
                textBox.IsFocused = false;
            }
            else if (_widgets[_focusedIndex] is Widgets.List list)
            {
                list.IsFocused = false;
            }
        }
    }

    private void CycleFocus()
    {
        var focusableWidgets = _widgets
            .Select((w, i) => new { Widget = w, Index = i })
            .Where(x => x.Widget is Widgets.Button or Widgets.TextBox or Widgets.List)
            .ToList();

        if (focusableWidgets.Count == 0)
            return;

        // Clear current focus
        ClearFocus();

        // Move to next focusable widget
        var currentPos = focusableWidgets.FindIndex(x => x.Index == _focusedIndex);
        var nextPos = (currentPos + 1) % focusableWidgets.Count;
        _focusedIndex = focusableWidgets[nextPos].Index;

        // Set new focus
        if (_widgets[_focusedIndex] is Widgets.Button nextButton)
        {
            nextButton.IsFocused = true;
        }
        else if (_widgets[_focusedIndex] is Widgets.TextBox nextTextBox)
        {
            nextTextBox.IsFocused = true;
        }
        else if (_widgets[_focusedIndex] is Widgets.List nextList)
        {
            nextList.IsFocused = true;
        }
    }
}
