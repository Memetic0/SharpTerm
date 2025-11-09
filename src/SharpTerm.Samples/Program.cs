using SharpTerm.Core;
using SharpTerm.Core.Widgets;
using SharpTerm.Samples.Demos.FormInput;
using SharpTerm.Samples.Demos.HelloWorld;
using SharpTerm.Samples.Demos.ProgressDemo;
using SharpTerm.Samples.Demos.TodoApp;
using SharpTerm.Samples.Demos.WidgetShowcase;

namespace SharpTerm.Samples;

public static class Program
{
    private static readonly List<(string Name, string Description, Action Demo)> _demos = new()
    {
        ("Hello World", "Simple text display demo", HelloWorldDemo.Run),
        ("Widget Showcase", "Interactive widget demonstration", WidgetShowcaseDemo.Run),
        ("TODO App", "Full-featured task management app", TodoAppDemo.Run),
        ("Progress Demo", "Progress bar demonstration", ProgressDemo.Run),
        ("Form Input", "User registration form example", FormInputDemo.Run),
    };

    public static void Main(string[] args)
    {
        while (true)
        {
            if (!ShowMainMenu())
                break;
        }

        Console.Clear();
        Console.WriteLine("Thank you for using SharpTerm! Goodbye!");
    }

    private static bool ShowMainMenu()
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
        int boxWidth = Math.Min(70, w - 10);
        int boxLeft = centerX - boxWidth / 2;

        // Title
        var titleBorder = new Border
        {
            Bounds = new Rectangle(boxLeft, 2, boxWidth, 3),
            Title = "SharpTerm Sample Applications",
            Style = BorderStyle.Double,
            ForegroundColor = Color.Cyan,
        };
        app.AddWidget(titleBorder);

        var subtitleLabel = new Label
        {
            Bounds = new Rectangle(boxLeft + 2, 3, boxWidth - 4, 1),
            Text = "A Modern Terminal UI Library for .NET",
            Alignment = TextAlignment.Center,
            ForegroundColor = Color.Yellow,
        };
        app.AddWidget(subtitleLabel);

        // Demo list
        var listBorder = new Border
        {
            Bounds = new Rectangle(boxLeft, 6, boxWidth, _demos.Count + 4),
            Title = "Select a Demo",
            Style = BorderStyle.Rounded,
            ForegroundColor = Color.Green,
        };
        app.AddWidget(listBorder);

        var demoList = new SharpTerm.Core.Widgets.List
        {
            Bounds = new Rectangle(boxLeft + 2, 7, boxWidth - 4, _demos.Count + 2),
            ForegroundColor = Color.White,
            IsFocused = true,
            SelectedColor = Color.Magenta,
        };

        // Format demo items with descriptions
        var demoItems = _demos
            .Select((d, i) => $"{i + 1}. {d.Name.PadRight(18)} - {d.Description}")
            .ToList();
        demoList.SetItems(demoItems);

        int? selectedDemoIndex = null;
        bool shouldExit = false;

        demoList.ItemActivated += (s, index) =>
        {
            if (index >= 0 && index < _demos.Count)
            {
                selectedDemoIndex = index;
                app.Stop();
            }
        };

        app.AddWidget(demoList);

        // Instructions
        var instructionsLabel = new Label
        {
            Bounds = new Rectangle(boxLeft, 6 + _demos.Count + 5, boxWidth, 1),
            Text = "??: Navigate | Enter: Run Demo | ESC: Exit Application",
            Alignment = TextAlignment.Center,
            ForegroundColor = Color.DarkGray,
        };
        app.AddWidget(instructionsLabel);

        // Exit button (hidden, just for ESC handling)
        var exitButton = new Button
        {
            Bounds = new Rectangle(0, 0, 0, 0),
            Text = "",
            InvokeKey = ConsoleKey.Escape,
        };
        exitButton.Click += (s, e) =>
        {
            shouldExit = true;
            app.Stop();
        };
        app.AddWidget(exitButton);

        app.Run();

        // If exit was requested, return false to exit program
        if (shouldExit)
            return false;

        // If a demo was selected, run it
        if (selectedDemoIndex.HasValue)
        {
            Console.Clear();
            try
            {
                _demos[selectedDemoIndex.Value].Demo();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error running demo: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey(true);
            }
        }

        return true; // Continue showing menu
    }
}
