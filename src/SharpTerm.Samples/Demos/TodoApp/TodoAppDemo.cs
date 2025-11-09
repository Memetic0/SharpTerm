using SharpTerm.Core;
using SharpTerm.Core.Theming;
using SharpTerm.Core.Widgets;

namespace SharpTerm.Samples.Demos.TodoApp;

public static class TodoAppDemo
{
    public static void Run()
    {
#if DEBUG
        var app = new Application(debugMode: true);
#else
        var app = new Application();
#endif

        // Use VirtualList for better performance with large todo lists
        var todos = new List<string>
        {
            "Learn SharpTerm",
            "Build TUI app",
            "Deploy to NuGet",
            "Add tests",
            "Write documentation",
            "Create examples",
            "Optimize performance",
            "Add more widgets",
            "Publish release",
        };

        // Add more todos to demonstrate VirtualList performance
        for (int i = 10; i <= 100; i++)
        {
            todos.Add($"Todo item #{i}");
        }

        int w = Console.WindowWidth;
        int h = Console.WindowHeight;
        int centerX = w / 2;
        int boxWidth = Math.Min(60, w - 20);
        int boxLeft = centerX - boxWidth / 2;

        // Title border
        var titleBorder = new Border
        {
            Bounds = new Rectangle(boxLeft, 1, boxWidth, 3),
            Title = "SharpTerm TODO App",
            Style = BorderStyle.Double,
            ForegroundColor = Color.Magenta,
            BackgroundColor = Color.Black
        };

        // Title
        var titleLabel = new Label
        {
            Bounds = new Rectangle(boxLeft + 2, 2, boxWidth - 4, 1),
            Text = "📋 Task Manager",
            Alignment = TextAlignment.Center,
            ForegroundColor = Color.Yellow,
            BackgroundColor = Color.Black
        };

        // List border
        var listBorder = new Border
        {
            Bounds = new Rectangle(boxLeft, 5, boxWidth, h - 11),
            Title = "Tasks",
            Style = BorderStyle.Rounded,
            ForegroundColor = Color.Cyan,
            BackgroundColor = Color.Black
        };

        // Use VirtualList for efficient rendering of many items
        var todoList = new VirtualList
        {
            Bounds = new Rectangle(boxLeft + 2, 6, boxWidth - 4, h - 13),
            IsFocused = true,
            ShowScrollbar = true,
            SelectedColor = Color.Green,
            AlternateRowColor = Color.DarkGray,
            ForegroundColor = Color.White,
            BackgroundColor = Color.Black
        };
        todoList.SetItems(todos.Select((t, i) => $"{i + 1}. {t}"));

        // Status label
        var statusLabel = new Label
        {
            Bounds = new Rectangle(boxLeft, h - 6, boxWidth, 1),
            Text = $"Total: {todos.Count} items (VirtualList handles millions!)",
            Alignment = TextAlignment.Center,
            ForegroundColor = Color.Cyan,
            BackgroundColor = Color.Black
        };

        // Buttons
        var addButton = new Button
        {
            Bounds = new Rectangle(boxLeft + 2, h - 4, 15, 1),
            Text = "Add [A]",
            Style = ButtonStyle.Rounded,
            ForegroundColor = Color.Green,
            BackgroundColor = Color.Black,
            InvokeKey = ConsoleKey.A
        };

        var removeButton = new Button
        {
            Bounds = new Rectangle(boxLeft + 19, h - 4, 18, 1),
            Text = "Remove [Del]",
            Style = ButtonStyle.Rounded,
            ForegroundColor = Color.Red,
            BackgroundColor = Color.Black,
            InvokeKey = ConsoleKey.Delete
        };

        var detailsButton = new Button
        {
            Bounds = new Rectangle(boxLeft + 39, h - 4, 20, 1),
            Text = "Details [D]",
            Style = ButtonStyle.Rounded,
            ForegroundColor = Color.Yellow,
            BackgroundColor = Color.Black,
            InvokeKey = ConsoleKey.D
        };

        // Add widgets (borders first for proper layering)
        app.AddWidget(titleBorder);
        app.AddWidget(titleLabel);
        app.AddWidget(listBorder);
        app.AddWidget(todoList);
        app.AddWidget(statusLabel);
        app.AddWidget(addButton);
        app.AddWidget(removeButton);
        app.AddWidget(detailsButton);

        // Instructions
        var instructions = new Label
        {
            Bounds = new Rectangle(boxLeft, h - 2, boxWidth, 1),
            Text = "TAB: Navigate | ↑↓/Scroll: Move | Enter: Activate | D: Details | A: Add | Del: Remove | ESC: Menu",
            Alignment = TextAlignment.Center,
            ForegroundColor = Color.Magenta,
            BackgroundColor = Color.Black
        };
        app.AddWidget(instructions);

        // Details dialog (initially hidden)
        var detailsDialog = new Dialog
        {
            Bounds = new Rectangle(w / 2 - 30, h / 2 - 5, 60, 10),
            Title = "Task Details",
            Message = "",
            Style = BorderStyle.Double,
            ForegroundColor = Color.Cyan,
            BackgroundColor = Color.Blue,
            Visible = false
        };
        app.AddWidget(detailsDialog);

        // Button handlers
        addButton.Click += (s, e) =>
        {
            todos.Add($"New task #{todos.Count + 1}");
            todoList.SetItems(todos.Select((t, i) => $"{i + 1}. {t}"));
            statusLabel.Text = $"Total: {todos.Count} items (VirtualList handles millions!)";
        };

        removeButton.Click += (s, e) =>
        {
            if (todos.Count > 0 && todoList.SelectedIndex >= 0 && todoList.SelectedIndex < todos.Count)
            {
                todos.RemoveAt(todoList.SelectedIndex);
                todoList.SetItems(todos.Select((t, i) => $"{i + 1}. {t}"));
                statusLabel.Text = $"Total: {todos.Count} items (VirtualList handles millions!)";
            }
        };

        detailsButton.Click += (s, e) =>
        {
            if (todoList.SelectedIndex >= 0 && todoList.SelectedIndex < todos.Count)
            {
                var task = todos[todoList.SelectedIndex];
                detailsDialog.Title = "Task Details";
                detailsDialog.Message = $"Task #{todoList.SelectedIndex + 1}\n\n{task}\n\nCreated: Today\nStatus: Pending\n\nPress ESC or Enter to close";
                detailsDialog.Visible = true;
                statusLabel.Text = $"Viewing details for: {task}";
                statusLabel.ForegroundColor = Color.Yellow;
            }
        };

        todoList.ItemActivated += (s, index) =>
        {
            Console.Title = $"ItemActivated fired! Index={index}, Count={todos.Count}";
            if (index >= 0 && index < todos.Count)
            {
                var task = todos[index];
                statusLabel.Text = $"✓ Activated: {task}";
                statusLabel.ForegroundColor = Color.Blue;
            }
            else
            {
                statusLabel.Text = $"Error: Index {index} out of range (Count={todos.Count})";
                statusLabel.ForegroundColor = Color.Red;
            }
        };

        // Handle resize
        app.Resized += (s, e) =>
        {
            int newW = Console.WindowWidth;
            int newH = Console.WindowHeight;
            int newCenterX = newW / 2;
            int newBoxWidth = Math.Min(60, newW - 20);
            int newBoxLeft = newCenterX - newBoxWidth / 2;

            titleBorder.Bounds = new Rectangle(newBoxLeft, 1, newBoxWidth, 3);
            titleLabel.Bounds = new Rectangle(newBoxLeft + 2, 2, newBoxWidth - 4, 1);
            listBorder.Bounds = new Rectangle(newBoxLeft, 5, newBoxWidth, newH - 11);
            todoList.Bounds = new Rectangle(newBoxLeft + 2, 6, newBoxWidth - 4, newH - 13);
            statusLabel.Bounds = new Rectangle(newBoxLeft, newH - 6, newBoxWidth, 1);
            addButton.Bounds = new Rectangle(newBoxLeft + 2, newH - 4, 15, 1);
            removeButton.Bounds = new Rectangle(newBoxLeft + 19, newH - 4, 18, 1);
            detailsButton.Bounds = new Rectangle(newBoxLeft + 39, newH - 4, 20, 1);
            instructions.Bounds = new Rectangle(newBoxLeft, newH - 2, newBoxWidth, 1);
            app.RequestRedraw();
        };

        app.Run();
    }
}
