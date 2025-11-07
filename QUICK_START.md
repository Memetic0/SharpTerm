# SharpTerm Quick Start Guide

## Installation

Add SharpTerm to your project:

```bash
dotnet add package SharpTerm
```

Or add to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="SharpTerm" Version="1.0.0" />
</ItemGroup>
```

## Basic Terminal Output

The simplest way to use SharpTerm is through the terminal driver directly:

```csharp
using SharpTerm.Core;

using var driver = new AnsiTerminalDriver();
driver.Clear();
driver.SetCursorPosition(0, 0);
driver.Write("Hello, SharpTerm!", Color.Cyan, Color.Black);
driver.Flush();
Console.ReadKey();
```

## Creating an Application

For interactive applications, use the `Application` framework:

```csharp
using SharpTerm.Core;
using SharpTerm.Core.Widgets;

var app = new Application();

var border = new Border
{
    Bounds = new Rectangle(5, 2, 50, 10),
    Title = "My First App",
    Style = BorderStyle.Double,
    ForegroundColor = Color.Cyan
};

var label = new Label
{
    Bounds = new Rectangle(7, 4, 46, 1),
    Text = "Welcome to SharpTerm!",
    Alignment = TextAlignment.Center,
    ForegroundColor = Color.Yellow
};

var button = new Button
{
    Bounds = new Rectangle(20, 8, 20, 1),
    Text = "Click Me!",
    IsFocused = true
};

button.Click += (s, e) =>
{
    label.Text = "Button Clicked!";
    label.ForegroundColor = Color.Green;
};

app.AddWidget(border);
app.AddWidget(label);
app.AddWidget(button);

app.Run(); // Press ESC to exit
```

## Widget Reference

### Label

Displays static or dynamic text with customizable alignment:

```csharp
var label = new Label
{
    Bounds = new Rectangle(10, 5, 30, 3),
    Text = "Line 1\nLine 2\nLine 3",
    Alignment = TextAlignment.Center,
    ForegroundColor = Color.Green,
    BackgroundColor = Color.Black
};
app.AddWidget(label);
```

**Properties:**
- `Text` - Content to display (supports multi-line with `\n`)
- `Alignment` - Left, Center, or Right
- `ForegroundColor` - Text color
- `BackgroundColor` - Background color (use `Color.Transparent` for terminal default)

### Button

Interactive button that responds to keyboard and mouse input:

```csharp
var button = new Button
{
    Bounds = new Rectangle(10, 5, 15, 1),
    Text = "Save",
    IsFocused = true,
    InvokeKey = ConsoleKey.S // Optional custom key
};

button.Click += (sender, e) =>
{
    // Handle button click
    Console.WriteLine("Save button clicked!");
};

app.AddWidget(button);
```

**Properties:**
- `Text` - Button label (rendered as `[ Text ]`)
- `IsFocused` - Whether button has keyboard focus
- `InvokeKey` - Custom activation key (default: Enter)
- `ForegroundColor` - Text color
- `FocusedColor` - Color when focused (default: Yellow)

**Events:**
- `Click` - Raised when activated via keyboard (Enter/custom key) or mouse click

### Border

Decorative box with optional title:

```csharp
var border = new Border
{
    Bounds = new Rectangle(5, 2, 60, 15),
    Title = "Settings",
    Style = BorderStyle.Double,
    ForegroundColor = Color.Cyan,
    BackgroundColor = Color.Black
};
app.AddWidget(border);
```

**Properties:**
- `Title` - Optional text displayed in top border
- `Style` - Single, Double, or Rounded
- `ForegroundColor` - Border and title color
- `BackgroundColor` - Interior fill color

**Border Styles:**
- `BorderStyle.Single` - Single-line box drawing characters (┌─┐)
- `BorderStyle.Double` - Double-line box drawing characters (╔═╗)
- `BorderStyle.Rounded` - Rounded corners (╭─╮)

### ProgressBar

Visual progress indicator with percentage display:

```csharp
var progressBar = new ProgressBar
{
    Bounds = new Rectangle(10, 5, 40, 1),
    Value = 0,
    Maximum = 100,
    FilledColor = Color.Green,
    EmptyColor = Color.DarkGray,
    ShowPercentage = true
};

// Update progress
progressBar.Value = 75; // Shows "75%"

app.AddWidget(progressBar);
```

**Properties:**
- `Value` - Current progress value
- `Maximum` - Maximum value (default: 100)
- `FilledColor` - Color of completed portion
- `EmptyColor` - Color of remaining portion
- `ShowPercentage` - Display percentage text overlay

### TextBox

Full-featured text input with editing capabilities:

```csharp
var textBox = new TextBox
{
    Bounds = new Rectangle(10, 5, 30, 1),
    Placeholder = "Enter your name...",
    MaxLength = 50,
    IsFocused = true
};

textBox.TextChanged += (sender, text) =>
{
    Console.WriteLine($"Text changed: {text}");
};

app.AddWidget(textBox);
```

**Properties:**
- `Text` - Current text content
- `Placeholder` - Hint text when empty
- `MaxLength` - Maximum character limit (0 = unlimited)
- `PasswordChar` - Character for masking (e.g., '*' for password fields)
- `IsFocused` - Whether textbox has keyboard focus

**Events:**
- `TextChanged` - Raised when text is modified

**Keyboard Controls:**
- Arrow keys - Move cursor
- Home/End - Jump to start/end
- Backspace - Delete before cursor
- Delete - Delete at cursor
- Printable characters - Insert at cursor

### List

Scrollable item list with selection and activation:

```csharp
var list = new List
{
    Bounds = new Rectangle(10, 5, 40, 10),
    IsFocused = true,
    SelectedColor = Color.Blue,
    ShowScrollbar = true
};

list.SetItems(new[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" });

list.SelectionChanged += (sender, e) =>
{
    Console.WriteLine($"Selected: {list.SelectedIndex}");
};

list.ItemActivated += (sender, index) =>
{
    Console.WriteLine($"Activated item {index}: {list.Items[index]}");
};

app.AddWidget(list);
```

**Properties:**
- `Items` - Read-only collection of items
- `SelectedIndex` - Currently selected item index
- `IsFocused` - Whether list has keyboard focus
- `SelectedColor` - Background color for selected item
- `ShowScrollbar` - Display scrollbar when items exceed height

**Methods:**
- `AddItem(string)` - Add single item
- `RemoveItem(int)` - Remove item by index
- `SetItems(IEnumerable<string>)` - Replace all items
- `Clear()` - Remove all items

**Events:**
- `SelectionChanged` - Raised when selected item changes
- `ItemActivated` - Raised on Enter key or double-click

**Keyboard Controls:**
- Up/Down arrows - Move selection
- Home/End - Jump to first/last
- PageUp/PageDown - Move by page
- Enter - Activate selected item

**Mouse Controls:**
- Click - Select item
- Double-click - Activate item
- Scroll wheel - Move selection

## Application Controls

### Keyboard Navigation

- **Tab** - Cycle focus between focusable widgets (Button, TextBox, List)
- **Esc** - Exit application
- **Widget-specific keys** - See individual widget documentation

### Focus Management

Widgets that support focus:
- Button (responds to Enter or custom InvokeKey)
- TextBox (editing and cursor movement)
- List (selection and activation)

Set initial focus:

```csharp
var button = new Button { IsFocused = true };
```

Focus automatically cycles through focusable widgets with Tab key.

### Event Handling

All widgets support the `Changed` event for state updates:

```csharp
widget.Changed += (sender, e) =>
{
    // Widget state changed, UI will update automatically
};
```

Specific events:
- `Button.Click` - Button activation
- `TextBox.TextChanged` - Text modification
- `List.SelectionChanged` - Selection change
- `List.ItemActivated` - Item activation

### Colors

SharpTerm supports 24-bit RGB colors:

```csharp
// Predefined colors
Color.Black, Color.White, Color.Red, Color.Green, Color.Blue,
Color.Yellow, Color.Cyan, Color.Magenta, Color.Gray,
Color.DarkGray, Color.DarkRed, Color.DarkGreen, Color.DarkBlue,
Color.DarkYellow, Color.DarkCyan, Color.DarkMagenta

// Custom RGB colors
var customColor = new Color(128, 200, 255);

// Transparent background (uses terminal default)
widget.BackgroundColor = Color.Transparent;
```

## Advanced Usage

### Responsive Layouts

Handle terminal resize events:

```csharp
app.Resized += (sender, e) =>
{
    int w = Console.WindowWidth;
    int h = Console.WindowHeight;
    
    // Reposition and resize widgets
    border.Bounds = new Rectangle(2, 1, w - 4, h - 2);
    label.Bounds = new Rectangle(4, 3, w - 8, 1);
    
    app.RequestRedraw();
};
```

### Manual Rendering Control

Request full redraw:

```csharp
app.RequestRedraw(); // Redraws all widgets
```

Remove widgets dynamically:

```csharp
app.RemoveWidget(widget);
app.RequestRedraw();
```

### Debug Mode

Enable debug logging:

```csharp
var app = new Application(debugMode: true);
```

Debug output writes to `debug.log` in the application directory.

## Complete Example: Task Manager

```csharp
using SharpTerm.Core;
using SharpTerm.Core.Widgets;

var app = new Application();
var tasks = new List<string> { "Task 1", "Task 2", "Task 3" };

int w = Console.WindowWidth;
int h = Console.WindowHeight;

var border = new Border
{
    Bounds = new Rectangle(2, 1, w - 4, h - 4),
    Title = "Task Manager",
    Style = BorderStyle.Double,
    ForegroundColor = Color.Cyan
};

var taskList = new List
{
    Bounds = new Rectangle(4, 3, w - 8, h - 10),
    IsFocused = true
};
taskList.SetItems(tasks);

var addButton = new Button
{
    Bounds = new Rectangle(4, h - 6, 12, 1),
    Text = "Add"
};

addButton.Click += (s, e) =>
{
    tasks.Add($"Task {tasks.Count + 1}");
    taskList.SetItems(tasks);
};

var removeButton = new Button
{
    Bounds = new Rectangle(18, h - 6, 15, 1),
    Text = "Remove"
};

removeButton.Click += (s, e) =>
{
    if (tasks.Count > 0 && taskList.SelectedIndex >= 0)
    {
        tasks.RemoveAt(taskList.SelectedIndex);
        taskList.SetItems(tasks);
    }
};

app.AddWidget(border);
app.AddWidget(taskList);
app.AddWidget(addButton);
app.AddWidget(removeButton);

app.Run();
```

## Next Steps

- Explore the sample applications in `src/SharpTerm.Samples`
- Read `IMPLEMENTATION_GUIDE.md` for architecture details
- Check the test suite in `tests/SharpTerm.Tests` for usage patterns
- Experiment with custom widget combinations and layouts

## Common Issues

**Issue: Colors not displaying**
- Ensure terminal supports ANSI escape sequences
- Use Windows Terminal on Windows (not legacy console)
- Check terminal settings for 24-bit color support

**Issue: Input not responding**
- Verify widget has `IsFocused = true` or cycle with Tab
- Check that Application.Run() is called
- Ensure no blocking operations in event handlers

**Issue: Flickering or visual artifacts**
- Widget dirty tracking should eliminate most flicker
- Avoid calling RequestRedraw() excessively
- Use per-widget state changes instead of full redraws

**Issue: Mouse not working**
- Mouse support requires Windows Console API (Windows only currently)
- Linux/macOS mouse support planned for future release
- Use keyboard navigation as fallback
