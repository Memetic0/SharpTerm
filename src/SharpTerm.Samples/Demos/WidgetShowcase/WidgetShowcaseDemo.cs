using SharpTerm.Core;
using SharpTerm.Core.Theming;
using SharpTerm.Core.Widgets;

namespace SharpTerm.Samples.Demos.WidgetShowcase;

public static class WidgetShowcaseDemo
{
    public static void Run()
    {
        // Use new dependency injection and services
        var services = new Core.Services.ApplicationServices();
        var app = new Application(services.TerminalDriver);
        var themeManager = services.ThemeManager;

        // Apply Dark theme
        themeManager.SetTheme(Theme.Dark);

        int w = Console.WindowWidth;
        int h = Console.WindowHeight;
        int centerX = w / 2;
        int centerY = h / 2;

        var border = new Border
        {
            Bounds = new Rectangle(centerX - 25, centerY - 8, 50, 16),
            Title = "SharpTerm Widget Showcase",
            Style = BorderStyle.Double
        };

        var label = new Label
        {
            Bounds = new Rectangle(centerX - 23, centerY - 6, 46, 1),
            Text = "Welcome to SharpTerm!",
            Alignment = TextAlignment.Center
        };

        var button = new Button
        {
            Bounds = new Rectangle(centerX - 10, centerY - 3, 20, 1),
            Text = "Click Me!",
            IsFocused = true
        };

        button.Click += (s, e) =>
        {
            label.Text = "Button Clicked!";
            label.ForegroundColor = Color.Green;
        };

        var progressBar = new ProgressBar
        {
            Bounds = new Rectangle(centerX - 23, centerY, 46, 1),
            Value = 65,
            ShowPercentage = true,
            UseGradient = true
        };

        var textBox = new TextBox
        {
            Bounds = new Rectangle(centerX - 23, centerY + 3, 46, 1),
            Placeholder = "Type something here...",
            IsFocused = false
        };

        // Apply theme to all widgets
        themeManager.ApplyTheme(border);
        themeManager.ApplyTheme(label);
        themeManager.ApplyTheme(button);
        themeManager.ApplyTheme(progressBar);
        themeManager.ApplyTheme(textBox);

        app.AddWidget(border);
        app.AddWidget(label);
        app.AddWidget(button);
        app.AddWidget(progressBar);
        app.AddWidget(textBox);

        var exitLabel = new Label
        {
            Bounds = new Rectangle(centerX - 28, centerY + 9, 56, 1),
            Text = "Press ESC to return to menu | TAB to cycle focus",
            Alignment = TextAlignment.Center,
            ForegroundColor = Color.DarkGray
        };
        app.AddWidget(exitLabel);

        // Handle resize
        app.Resized += (s, e) =>
        {
            int newW = Console.WindowWidth;
            int newH = Console.WindowHeight;
            int newCenterX = newW / 2;
            int newCenterY = newH / 2;

            border.Bounds = new Rectangle(newCenterX - 25, newCenterY - 8, 50, 16);
            label.Bounds = new Rectangle(newCenterX - 23, newCenterY - 6, 46, 1);
            button.Bounds = new Rectangle(newCenterX - 10, newCenterY - 3, 20, 1);
            progressBar.Bounds = new Rectangle(newCenterX - 23, newCenterY, 46, 1);
            textBox.Bounds = new Rectangle(newCenterX - 23, newCenterY + 3, 46, 1);
            exitLabel.Bounds = new Rectangle(newCenterX - 28, newCenterY + 9, 56, 1);
            app.RequestRedraw();
        };

        // Animate progress bar
        var timer = new System.Timers.Timer(50);
        timer.Elapsed += (s, e) =>
        {
            progressBar.Value = (progressBar.Value + 1) % 101;
        };
        timer.Start();

        app.Run();
        timer.Stop();
    }
}
