namespace SharpTerm.Core.ApplicationLogic;

/// <summary>
/// Application input handling logic.
/// </summary>
internal static class InputProcessor
{
    internal static bool ProcessInput(
        ITerminalDriver driver,
        List<Widget> widgets,
        HashSet<Widget> dirtyWidgets,
        int focusedIndex,
        Action<string> debugLog,
        Action stopApplication,
        out int newFocusedIndex,
        out bool needsFullRedraw
    )
    {
        newFocusedIndex = focusedIndex;
        needsFullRedraw = false;

        // Process ALL pending input events in the queue
        const int maxEventsPerFrame = 50;
        int eventsProcessed = 0;

        while (eventsProcessed < maxEventsPerFrame && driver.HasInputEvents)
        {
            var eventType = driver.ReadConsoleEvent(out var keyInfo, out var mouseEvent);

            if (eventType == ConsoleEventType.None)
            {
                break; // No more events
            }

            eventsProcessed++;

            if (eventType == ConsoleEventType.Keyboard && keyInfo.HasValue)
            {
                var key = keyInfo.Value;
                debugLog($"Key pressed: {key.Key} (Char='{key.KeyChar}')");

                // ESC has absolute priority - stop immediately
                if (key.Key == ConsoleKey.Escape)
                {
                    debugLog("ESC pressed - stopping application");
                    stopApplication();
                    return true; // Indicate app should stop
                }

                // Tab navigation
                if (key.Key == ConsoleKey.Tab)
                {
                    debugLog("TAB pressed - cycling focus");
                    newFocusedIndex = FocusManager.CycleFocus(widgets, newFocusedIndex);
                    needsFullRedraw = true;
                    continue;
                }

                // Check for visible dialogs first (they have priority)
                bool dialogHandled = false;
                for (int i = widgets.Count - 1; i >= 0; i--)
                {
                    if (widgets[i] is Widgets.Dialog dialog && dialog.Visible)
                    {
                        bool handled = dialog.HandleKey(key);
                        if (handled)
                        {
                            debugLog($"Dialog handled key: {key.Key}");
                            lock (dirtyWidgets)
                            {
                                dirtyWidgets.Add(dialog);
                            }
                            dialogHandled = true;
                            break;
                        }
                    }
                }

                if (dialogHandled)
                {
                    continue;
                }

                // Check for global button shortcuts (buttons can be triggered even when not focused)
                bool buttonShortcutHandled = false;
                for (int i = 0; i < widgets.Count; i++)
                {
                    if (widgets[i] is Widgets.Button button && button.Visible)
                    {
                        if (key.Key == button.InvokeKey)
                        {
                            debugLog($"Global button shortcut: {button.InvokeKey} pressed for button '{button.Text}'");
                            button.InvokeClick();
                            lock (dirtyWidgets)
                            {
                                dirtyWidgets.Add(button);
                            }
                            buttonShortcutHandled = true;
                            break;
                        }
                    }
                }

                // If button shortcut was handled, don't pass to focused widget
                if (buttonShortcutHandled)
                {
                    continue;
                }

                // Pass to focused widget
                if (newFocusedIndex >= 0 && newFocusedIndex < widgets.Count)
                {
                    var widget = widgets[newFocusedIndex];
                    bool keyHandled = widget.HandleKey(key);
                    if (keyHandled)
                    {
                        debugLog($"Key handled by widget at index {newFocusedIndex}");
                        lock (dirtyWidgets)
                        {
                            dirtyWidgets.Add(widget);
                        }
                    }
                }
            }
            else if (eventType == ConsoleEventType.Mouse && mouseEvent != null)
            {
                if (mouseEvent.ScrollDelta != 0)
                {
                    debugLog(
                        $"Mouse scroll: Delta={mouseEvent.ScrollDelta} at X={mouseEvent.X}, Y={mouseEvent.Y}"
                    );
                    HandleMouseScroll(
                        widgets,
                        dirtyWidgets,
                        mouseEvent.X,
                        mouseEvent.Y,
                        mouseEvent.ScrollDelta,
                        debugLog,
                        out bool scrollNeedsRedraw
                    );
                    // Don't set needsFullRedraw for scroll - we handle it with dirty widgets
                }
                else
                {
                    debugLog($"Mouse click: X={mouseEvent.X}, Y={mouseEvent.Y}");
                    var oldFocusedIndex = newFocusedIndex;
                    HandleMouseClick(
                        widgets,
                        mouseEvent.X,
                        mouseEvent.Y,
                        debugLog,
                        out newFocusedIndex,
                        out needsFullRedraw
                    );
                    if (newFocusedIndex != -1 && newFocusedIndex != oldFocusedIndex)
                    {
                        FocusManager.ClearFocus(widgets, oldFocusedIndex);
                    }
                }
            }
            else if (eventType == ConsoleEventType.Other)
            {
                debugLog("Other console event consumed (key up, mouse move, focus, resize, etc)");
            }
        }

        if (eventsProcessed > 0)
        {
            debugLog($"Processed {eventsProcessed} input events this frame");
        }

        return false; // App should continue
    }

    private static void HandleMouseClick(
        List<Widget> widgets,
        int x,
        int y,
        Action<string> debugLog,
        out int newFocusedIndex,
        out bool needsFullRedraw
    )
    {
        newFocusedIndex = -1;
        needsFullRedraw = false;

        // Check if any clickable widget was clicked
        for (int i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
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
                debugLog($"Button clicked at ({x}, {y}) - widget index {i}");
                newFocusedIndex = i;
                button.IsFocused = true;
                button.InvokeClick();
                needsFullRedraw = true;
                return;
            }
            else if (widget is Widgets.TextBox textBox)
            {
                debugLog($"TextBox clicked at ({x}, {y}) - widget index {i}");
                newFocusedIndex = i;
                textBox.IsFocused = true;
                needsFullRedraw = true;
                return;
            }
            else if (widget is Widgets.List list)
            {
                debugLog($"List clicked at ({x}, {y}) - widget index {i}");
                newFocusedIndex = i;
                list.IsFocused = true;
                int relativeY = y - widget.Bounds.Y;
                list.HandleClick(relativeY);
                needsFullRedraw = true;
                return;
            }
            else if (widget is Widgets.VirtualList virtualList)
            {
                debugLog($"VirtualList clicked at ({x}, {y}) - widget index {i}");
                newFocusedIndex = i;
                virtualList.IsFocused = true;
                int relativeY = y - widget.Bounds.Y;
                virtualList.HandleClick(relativeY);
                needsFullRedraw = true;
                return;
            }
        }

        debugLog($"Click at ({x}, {y}) - no widget hit");
    }

    private static void HandleMouseScroll(
        List<Widget> widgets,
        HashSet<Widget> dirtyWidgets,
        int x,
        int y,
        int scrollDelta,
        Action<string> debugLog,
        out bool needsFullRedraw
    )
    {
        needsFullRedraw = false;

        // Check if scrolling over a List widget
        for (int i = 0; i < widgets.Count; i++)
        {
            var widget = widgets[i];
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
                debugLog($"List scrolled at ({x}, {y}) - widget index {i}, delta={scrollDelta}");
                list.HandleScroll(scrollDelta);
                lock (dirtyWidgets)
                {
                    dirtyWidgets.Add(list);
                }
                return;
            }
            else if (widget is Widgets.VirtualList virtualList)
            {
                debugLog($"VirtualList scrolled at ({x}, {y}) - widget index {i}, delta={scrollDelta}");
                virtualList.HandleScroll(scrollDelta);
                lock (dirtyWidgets)
                {
                    dirtyWidgets.Add(virtualList);
                }
                return;
            }
        }
    }
}
