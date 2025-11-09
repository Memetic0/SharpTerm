namespace SharpTerm.Core.ApplicationLogic;

/// <summary>
/// Application focus management logic.
/// </summary>
internal static class FocusManager
{
    internal static int InitializeFocus(List<Widget> widgets)
    {
        // Set initial focus to first focusable widget
        var firstFocusable = widgets
            .Select((w, i) => new { Widget = w, Index = i })
            .FirstOrDefault(x => x.Widget is Widgets.Button or Widgets.TextBox or Widgets.List or Widgets.VirtualList);

        if (firstFocusable != null)
        {
            var focusedIndex = firstFocusable.Index;
            if (firstFocusable.Widget is Widgets.Button button)
            {
                button.IsFocused = true;
            }
            else if (firstFocusable.Widget is Widgets.TextBox textBox)
            {
                textBox.IsFocused = true;
            }
            else if (firstFocusable.Widget is Widgets.List list)
            {
                list.IsFocused = true;
            }
            else if (firstFocusable.Widget is Widgets.VirtualList virtualList)
            {
                virtualList.IsFocused = true;
            }
            return focusedIndex;
        }

        return -1;
    }

    internal static void ClearFocus(List<Widget> widgets, int focusedIndex)
    {
        if (focusedIndex >= 0 && focusedIndex < widgets.Count)
        {
            if (widgets[focusedIndex] is Widgets.Button button)
            {
                button.IsFocused = false;
            }
            else if (widgets[focusedIndex] is Widgets.TextBox textBox)
            {
                textBox.IsFocused = false;
            }
            else if (widgets[focusedIndex] is Widgets.List list)
            {
                list.IsFocused = false;
            }
            else if (widgets[focusedIndex] is Widgets.VirtualList virtualList)
            {
                virtualList.IsFocused = false;
            }
        }
    }

    internal static int CycleFocus(List<Widget> widgets, int focusedIndex)
    {
        var focusableWidgets = widgets
            .Select((w, i) => new { Widget = w, Index = i })
            .Where(x => x.Widget is Widgets.Button or Widgets.TextBox or Widgets.List or Widgets.VirtualList)
            .ToList();

        if (focusableWidgets.Count == 0)
            return focusedIndex;

        // Clear current focus
        ClearFocus(widgets, focusedIndex);

        // Move to next focusable widget
        var currentPos = focusableWidgets.FindIndex(x => x.Index == focusedIndex);
        var nextPos = (currentPos + 1) % focusableWidgets.Count;
        var newFocusedIndex = focusableWidgets[nextPos].Index;

        // Set new focus
        if (widgets[newFocusedIndex] is Widgets.Button nextButton)
        {
            nextButton.IsFocused = true;
        }
        else if (widgets[newFocusedIndex] is Widgets.TextBox nextTextBox)
        {
            nextTextBox.IsFocused = true;
        }
        else if (widgets[newFocusedIndex] is Widgets.List nextList)
        {
            nextList.IsFocused = true;
        }
        else if (widgets[newFocusedIndex] is Widgets.VirtualList nextVirtualList)
        {
            nextVirtualList.IsFocused = true;
        }

        return newFocusedIndex;
    }
}
