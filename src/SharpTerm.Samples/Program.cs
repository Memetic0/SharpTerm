using SharpTerm.Core;
using SharpTerm.Core.Widgets;

// Simple Hello World Demo
static void HelloWorldDemo()
{
    using var driver = new AnsiTerminalDriver();
    driver.Clear();
    driver.SetCursorPosition(0, 0);
    driver.Write("Hello, SharpTerm!", Color.Cyan, Color.Black);
    driver.SetCursorPosition(0, 2);
    driver.Write("Press any key to continue...", Color.Yellow, Color.Black);
    driver.Flush();
    Console.ReadKey(true);
}

// Widget Showcase Demo
static void WidgetShowcaseDemo()
{
#if DEBUG
    var app = new Application(debugMode: true);
#else
    var app = new Application();
#endif
    
    Border? border = null;
    Label? label = null;
    Button? button = null;
    Label? exitLabel = null;
    
    void LayoutWidgets()
    {
        int w = Console.WindowWidth;
        int h = Console.WindowHeight;
        int centerX = w / 2;
        int centerY = h / 2;
        
        if (border == null)
        {
            border = new Border
            {
                Bounds = new Rectangle(centerX - 25, centerY - 5, 50, 10),
                Title = "SharpTerm Demo",
                Style = BorderStyle.Double,
                ForegroundColor = Color.Cyan
            };
            app.AddWidget(border);
        }
        else
        {
            border.Bounds = new Rectangle(centerX - 25, centerY - 5, 50, 10);
        }
        
        if (label == null)
        {
            label = new Label
            {
                Bounds = new Rectangle(centerX - 23, centerY - 3, 46, 1),
                Text = "Welcome to SharpTerm!",
                Alignment = TextAlignment.Center,
                ForegroundColor = Color.Yellow
            };
            app.AddWidget(label);
        }
        else
        {
            label.Bounds = new Rectangle(centerX - 23, centerY - 3, 46, 1);
        }
        
        if (button == null)
        {
            button = new Button
            {
                Bounds = new Rectangle(centerX - 10, centerY + 1, 20, 1),
                Text = "Click Me!",
                IsFocused = true
            };
            button.Click += (s, e) =>
            {
                label.Text = "Button Clicked!";
                label.ForegroundColor = Color.Green;
            };
            app.AddWidget(button);
        }
        else
        {
            button.Bounds = new Rectangle(centerX - 10, centerY + 1, 20, 1);
        }
        
        if (exitLabel == null)
        {
            exitLabel = new Label
            {
                Bounds = new Rectangle(centerX - 25, centerY + 6, 50, 1),
                Text = "Press ESC to exit, ENTER to click, TAB to navigate",
                ForegroundColor = Color.DarkGray
            };
            app.AddWidget(exitLabel);
        }
        else
        {
            exitLabel.Bounds = new Rectangle(centerX - 25, centerY + 6, 50, 1);
        }
    }
    
    LayoutWidgets();
    app.Resized += (s, e) => LayoutWidgets();
    
    app.Run();
}

// Helper to show task details overlay
static void ShowTaskDetails(Application app, int boxLeft, int boxWidth, string task, int taskNumber, int totalTasks, Label statusLabel)
{
    // Create a black background panel to cover what's behind
    var backgroundPanel = new Label
    {
        Bounds = new Rectangle(boxLeft + 5, 7, boxWidth - 10, 7),
        Text = string.Empty,
        ForegroundColor = Color.Black,
        BackgroundColor = Color.Black
    };
    
    var detailBorder = new Border
    {
        Bounds = new Rectangle(boxLeft + 5, 7, boxWidth - 10, 7),
        Title = "Task Details",
        Style = BorderStyle.Double,
        ForegroundColor = Color.Yellow,
        BackgroundColor = Color.Black
    };
    
    var detailLabel = new Label
    {
        Bounds = new Rectangle(boxLeft + 7, 9, boxWidth - 14, 3),
        Text = $"Task #{taskNumber} of {totalTasks}\n\n{task}\n\nPress ESC to close...",
        ForegroundColor = Color.White,
        BackgroundColor = Color.Black
    };
    
    // Create close button to handle ESC key and dismiss the dialog
    var closeButton = new Button
    {
        Bounds = new Rectangle(0, 0, 0, 0), // Hidden, just for ESC handling
        Text = "",
        InvokeKey = ConsoleKey.Escape
    };
    
    closeButton.Click += (s, e) =>
    {
        app.RemoveWidget(closeButton);
        app.RemoveWidget(detailLabel);
        app.RemoveWidget(detailBorder);
        app.RemoveWidget(backgroundPanel);
        statusLabel.Text = "Tab: Navigate | ↑↓: Scroll | Enter: Details | Del: Remove | Esc: Exit";
        statusLabel.ForegroundColor = Color.Yellow;
        app.RequestRedraw();
    };
    
    app.AddWidget(backgroundPanel);
    app.AddWidget(detailBorder);
    app.AddWidget(detailLabel);
    app.AddWidget(closeButton);
    
    statusLabel.Text = "Showing task details - Press ESC to close";
    statusLabel.ForegroundColor = Color.Cyan;
    
    app.RequestRedraw();
}

// TODO App Demo
static void TodoAppDemo()
{
#if DEBUG
    var app = new Application(debugMode: true);
#else
    var app = new Application();
#endif
    var todos = new List<string> { "Learn SharpTerm", "Build TUI app", "Deploy to NuGet", "Add tests", "Write documentation", "Create examples", "Optimize performance", "Add more widgets", "Publish release" };
    
    int w = Console.WindowWidth;
    int h = Console.WindowHeight;
    int centerX = w / 2;
    int boxWidth = Math.Min(60, w - 20);
    int boxLeft = centerX - boxWidth / 2;
    
    // Title border
    app.AddWidget(new Border
    {
        Bounds = new Rectangle(boxLeft, 1, boxWidth, 3),
        Title = "SharpTerm TODO App",
        Style = BorderStyle.Double,
        ForegroundColor = Color.Cyan
    });
    
    // TODO list display with scrollable list widget
    var listBorder = new Border
    {
        Bounds = new Rectangle(boxLeft, 4, boxWidth, 11),
        Title = "Tasks (Use ↑↓ to scroll)",
        ForegroundColor = Color.Green
    };
    app.AddWidget(listBorder);
    
    // Status bar (declare first so it can be used in event handlers)
    var statusLabel = new Label
    {
        Bounds = new Rectangle(boxLeft, 18, boxWidth, 1),
        Text = "Tab: Navigate | ↑↓: Scroll | Enter: Details | Del: Remove | Esc: Exit",
        ForegroundColor = Color.Yellow
    };
    app.AddWidget(statusLabel);
    
    var todoList = new SharpTerm.Core.Widgets.List
    {
        Bounds = new Rectangle(boxLeft + 2, 5, boxWidth - 4, 9),
        ForegroundColor = Color.White,
        IsFocused = true
    };
    todoList.SetItems(todos.Select((t, i) => $"{i + 1}. {t}"));
    
    // Handle item activation (Enter or double-click)
    todoList.ItemActivated += (s, index) =>
    {
        if (index >= 0 && index < todos.Count)
        {
            var task = todos[index];
            ShowTaskDetails(app, boxLeft, boxWidth, task, index + 1, todos.Count, statusLabel);
        }
    };
    
    app.AddWidget(todoList);
    
    void UpdateTodoList()
    {
        todoList.SetItems(todos.Select((t, i) => $"{i + 1}. {t}"));
    }
    
    // Buttons with better widths
    var addButton = new Button
    {
        Bounds = new Rectangle(boxLeft + 2, 15, 14, 1),
        Text = "Add Task"
    };
    addButton.Click += (s, e) =>
    {
        todos.Add($"New Task #{todos.Count + 1}");
        UpdateTodoList();
        statusLabel.Text = "Task added!";
        statusLabel.ForegroundColor = Color.Green;
    };
    app.AddWidget(addButton);
    
    var removeButton = new Button
    {
        Bounds = new Rectangle(boxLeft + 18, 15, 20, 1),
        Text = "Remove Selected",
        InvokeKey = ConsoleKey.Delete
    };
    removeButton.Click += (s, e) =>
    {
        if (todos.Count > 0 && todoList.SelectedIndex >= 0 && todoList.SelectedIndex < todos.Count)
        {
            todos.RemoveAt(todoList.SelectedIndex);
            UpdateTodoList();
            statusLabel.Text = "Task removed!";
            statusLabel.ForegroundColor = Color.Red;
        }
    };
    app.AddWidget(removeButton);
    
    var exitButton = new Button
    {
        Bounds = new Rectangle(boxLeft + 40, 15, 10, 1),
        Text = "Exit"
    };
    exitButton.Click += (s, e) => app.Stop();
    app.AddWidget(exitButton);
    
    app.Run();
}

// Progress Bar Demo
static void ProgressBarDemo()
{
#if DEBUG
    var app = new Application(debugMode: true);
#else
    var app = new Application();
#endif
    
    int w = Console.WindowWidth;
    int h = Console.WindowHeight;
    int centerX = w / 2;
    int centerY = h / 2;
    int boxWidth = Math.Min(60, w - 20);
    int boxLeft = centerX - boxWidth / 2;
    
    var border = new Border
    {
        Bounds = new Rectangle(boxLeft, centerY - 6, boxWidth, 12),
        Title = "Progress Demo",
        Style = BorderStyle.Rounded,
        ForegroundColor = Color.Cyan
    };
    app.AddWidget(border);
    
    var titleLabel = new Label
    {
        Bounds = new Rectangle(boxLeft + 2, centerY - 4, boxWidth - 4, 1),
        Text = "Simulating Task Progress...",
        Alignment = TextAlignment.Center,
        ForegroundColor = Color.Yellow
    };
    app.AddWidget(titleLabel);
    
    var progressBar1 = new ProgressBar
    {
        Bounds = new Rectangle(boxLeft + 2, centerY - 2, boxWidth - 4, 1),
        Value = 0,
        Maximum = 100,
        FilledColor = Color.Green,
        ShowPercentage = true
    };
    app.AddWidget(progressBar1);
    
    var progressBar2 = new ProgressBar
    {
        Bounds = new Rectangle(boxLeft + 2, centerY, boxWidth - 4, 1),
        Value = 0,
        Maximum = 100,
        FilledColor = Color.Blue,
        ShowPercentage = true
    };
    app.AddWidget(progressBar2);
    
    var statusLabel = new Label
    {
        Bounds = new Rectangle(boxLeft + 2, centerY + 2, boxWidth - 4, 1),
        Text = "Press SPACE to increment, ESC to exit",
        Alignment = TextAlignment.Center,
        ForegroundColor = Color.DarkGray
    };
    app.AddWidget(statusLabel);
    
    int btnWidth = (boxWidth - 8) / 2;
    var incrementButton = new Button
    {
        Bounds = new Rectangle(boxLeft + 2, centerY + 4, btnWidth, 1),
        Text = "Increment",
        IsFocused = true,
        InvokeKey = ConsoleKey.Spacebar
    };
    incrementButton.Click += (s, e) =>
    {
        if (progressBar1.Value < 100)
        {
            progressBar1.Value += 5;
        }
        if (progressBar2.Value < 100)
        {
            progressBar2.Value += 3;
        }
        
        if (progressBar1.Value >= 100 && progressBar2.Value >= 100)
        {
            statusLabel.Text = "Complete! Press ESC to exit";
            statusLabel.ForegroundColor = Color.Green;
        }
    };
    app.AddWidget(incrementButton);
    
    var closeButton = new Button
    {
        Bounds = new Rectangle(boxLeft + 4 + btnWidth, centerY + 4, btnWidth, 1),
        Text = "Close"
    };
    closeButton.Click += (s, e) => app.Stop();
    app.AddWidget(closeButton);
    
    app.Run();
}

// Form Input Demo
static void FormInputDemo()
{
#if DEBUG
    var app = new Application(debugMode: true);
#else
    var app = new Application();
#endif
    
    int w = Console.WindowWidth;
    int h = Console.WindowHeight;
    int centerX = w / 2;
    int centerY = h / 2;
    int boxWidth = Math.Min(60, w - 20);
    int boxLeft = centerX - boxWidth / 2;
    int inputWidth = boxWidth - 24;
    
    var border = new Border
    {
        Bounds = new Rectangle(boxLeft, centerY - 8, boxWidth, 16),
        Title = "User Registration Form",
        Style = BorderStyle.Double,
        ForegroundColor = Color.Cyan
    };
    app.AddWidget(border);
    
    app.AddWidget(new Label
    {
        Bounds = new Rectangle(boxLeft + 2, centerY - 6, 20, 1),
        Text = "Name:",
        ForegroundColor = Color.Yellow
    });
    
    var nameBox = new TextBox
    {
        Bounds = new Rectangle(boxLeft + 22, centerY - 6, inputWidth, 1),
        Placeholder = "Enter your name",
        MaxLength = 50
    };
    app.AddWidget(nameBox);
    
    app.AddWidget(new Label
    {
        Bounds = new Rectangle(boxLeft + 2, centerY - 4, 20, 1),
        Text = "Email:",
        ForegroundColor = Color.Yellow
    });
    
    var emailBox = new TextBox
    {
        Bounds = new Rectangle(boxLeft + 22, centerY - 4, inputWidth, 1),
        Placeholder = "your@email.com",
        MaxLength = 50
    };
    app.AddWidget(emailBox);
    
    app.AddWidget(new Label
    {
        Bounds = new Rectangle(boxLeft + 2, centerY - 2, 20, 1),
        Text = "Password:",
        ForegroundColor = Color.Yellow
    });
    
    var passwordBox = new TextBox
    {
        Bounds = new Rectangle(boxLeft + 22, centerY - 2, inputWidth, 1),
        Placeholder = "Enter password",
        PasswordChar = '*',
        MaxLength = 30
    };
    app.AddWidget(passwordBox);
    
    var resultLabel = new Label
    {
        Bounds = new Rectangle(boxLeft + 2, centerY + 1, boxWidth - 4, 3),
        Text = "",
        ForegroundColor = Color.White
    };
    app.AddWidget(resultLabel);
    
    int btnWidth = (boxWidth - 8) / 2;
    var submitButton = new Button
    {
        Bounds = new Rectangle(boxLeft + 2, centerY + 5, btnWidth, 1),
        Text = "Submit"
    };
    submitButton.Click += (s, e) =>
    {
        if (string.IsNullOrWhiteSpace(nameBox.Text))
        {
            resultLabel.Text = "Error: Name is required!";
            resultLabel.ForegroundColor = Color.Red;
        }
        else if (string.IsNullOrWhiteSpace(emailBox.Text))
        {
            resultLabel.Text = "Error: Email is required!";
            resultLabel.ForegroundColor = Color.Red;
        }
        else if (passwordBox.Text.Length < 6)
        {
            resultLabel.Text = "Error: Password must be at least 6 characters!";
            resultLabel.ForegroundColor = Color.Red;
        }
        else
        {
            resultLabel.Text = $"Success!\nName: {nameBox.Text}\nEmail: {emailBox.Text}\nPassword: {new string('*', passwordBox.Text.Length)}";
            resultLabel.ForegroundColor = Color.Green;
        }
    };
    app.AddWidget(submitButton);
    
    var cancelButton = new Button
    {
        Bounds = new Rectangle(boxLeft + 4 + btnWidth, centerY + 5, btnWidth, 1),
        Text = "Cancel"
    };
    cancelButton.Click += (s, e) => app.Stop();
    app.AddWidget(cancelButton);
    
    app.Run();
}

// Main menu
Console.Clear();
Console.WriteLine("SharpTerm Sample Applications");
Console.WriteLine("==============================\n");
Console.WriteLine("1. Hello World Demo");
Console.WriteLine("2. Widget Showcase");
Console.WriteLine("3. TODO App");
Console.WriteLine("4. Progress Bar Demo");
Console.WriteLine("5. Form Input Demo");
Console.WriteLine("0. Exit");
Console.Write("\nSelect demo (0-5): ");

var choice = Console.ReadKey(true).KeyChar;
Console.Clear();

switch (choice)
{
    case '1':
        HelloWorldDemo();
        break;
    case '2':
        WidgetShowcaseDemo();
        break;
    case '3':
        TodoAppDemo();
        break;
    case '4':
        ProgressBarDemo();
        break;
    case '5':
        FormInputDemo();
        break;
    default:
        Console.WriteLine("Goodbye!");
        break;
}

