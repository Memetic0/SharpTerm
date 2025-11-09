using SharpTerm.Core.Performance;

namespace SharpTerm.Core.ApplicationLogic;

/// <summary>
/// Application input handling logic.
/// </summary>
internal static class InputProcessor
{
    internal static bool ProcessInput(
        ITerminalDriver driver,
        List<Widget> widgets,
        LockFreeDirtyTracker dirtyWidgets,
        SpatialIndex spatialIndex,
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
                            dirtyWidgets.MarkDirty(dialog);
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
                            dirtyWidgets.MarkDirty(button);
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
                        dirtyWidgets.MarkDirty(widget);
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
                        spatialIndex,
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
                        spatialIndex,
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
        SpatialIndex spatialIndex,
        int x,
        int y,
        Action<string> debugLog,
        out int newFocusedIndex,
        out bool needsFullRedraw
    )
    {
        newFocusedIndex = -1;
        needsFullRedraw = false;

        // Use spatial index for fast hit testing
        var hitWidgets = spatialIndex.Query(x, y);

        // Process widgets in reverse order (topmost first)
        for (int i = hitWidgets.Count - 1; i >= 0; i--)
        {
            var widget = hitWidgets[i];
            if (!widget.Visible)
                continue;

            // Find the widget index in the main list
            int widgetIndex = widgets.IndexOf(widget);
            if (widgetIndex == -1)
                continue;

            if (widget is Widgets.Button button)
            {
                debugLog($"Button clicked at ({x}, {y}) - widget index {widgetIndex}");
                newFocusedIndex = widgetIndex;
                button.IsFocused = true;
                button.InvokeClick();
                needsFullRedraw = true;
                return;
            }
            else if (widget is Widgets.TextBox textBox)
            {
                debugLog($"TextBox clicked at ({x}, {y}) - widget index {widgetIndex}");
                newFocusedIndex = widgetIndex;
                textBox.IsFocused = true;
                needsFullRedraw = true;
                return;
            }
            else if (widget is Widgets.List list)
            {
                debugLog($"List clicked at ({x}, {y}) - widget index {widgetIndex}");
                newFocusedIndex = widgetIndex;
                list.IsFocused = true;
                int relativeY = y - widget.Bounds.Y;
                list.HandleClick(relativeY);
                needsFullRedraw = true;
                return;
            }
            else if (widget is Widgets.VirtualList virtualList)
            {
                debugLog($"VirtualList clicked at ({x}, {y}) - widget index {widgetIndex}");
                newFocusedIndex = widgetIndex;
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
        LockFreeDirtyTracker dirtyWidgets,
        SpatialIndex spatialIndex,
        int x,
        int y,
        int scrollDelta,
        Action<string> debugLog,
        out bool needsFullRedraw
    )
    {
        needsFullRedraw = false;

        // Use spatial index for fast hit testing
        var hitWidgets = spatialIndex.Query(x, y);

        // Process widgets in reverse order (topmost first)
        for (int i = hitWidgets.Count - 1; i >= 0; i--)
        {
            var widget = hitWidgets[i];
            if (!widget.Visible)
                continue;

            // Find the widget index in the main list
            int widgetIndex = widgets.IndexOf(widget);
            if (widgetIndex == -1)
                continue;

            if (widget is Widgets.List list)
            {
                debugLog($"List scrolled at ({x}, {y}) - widget index {widgetIndex}, delta={scrollDelta}");
                list.HandleScroll(scrollDelta);
                dirtyWidgets.MarkDirty(list);
                return;
            }
            else if (widget is Widgets.VirtualList virtualList)
            {
                debugLog($"VirtualList scrolled at ({x}, {y}) - widget index {widgetIndex}, delta={scrollDelta}");
                virtualList.HandleScroll(scrollDelta);
                dirtyWidgets.MarkDirty(virtualList);
                return;
            }
        }
    }
}
