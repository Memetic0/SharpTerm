using SharpTerm.Core;
using SharpTerm.Core.Widgets;

namespace SharpTerm.Samples.Demos.ProgressDemo;

public static class ProgressDemo
{
    public static void Run()
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
            ForegroundColor = Color.Cyan,
        };
        app.AddWidget(border);

        var titleLabel = new Label
        {
            Bounds = new Rectangle(boxLeft + 2, centerY - 4, boxWidth - 4, 1),
            Text = "Simulating Task Progress...",
            Alignment = TextAlignment.Center,
            ForegroundColor = Color.Yellow,
        };
        app.AddWidget(titleLabel);

        var progressBar1 = new ProgressBar
        {
            Bounds = new Rectangle(boxLeft + 2, centerY - 2, boxWidth - 4, 1),
            Value = 0,
            Maximum = 100,
            FilledColor = Color.Green,
            ShowPercentage = true,
        };
        app.AddWidget(progressBar1);

        var progressBar2 = new ProgressBar
        {
            Bounds = new Rectangle(boxLeft + 2, centerY, boxWidth - 4, 1),
            Value = 0,
            Maximum = 100,
            FilledColor = Color.Blue,
            ShowPercentage = true,
        };
        app.AddWidget(progressBar2);

        var statusLabel = new Label
        {
            Bounds = new Rectangle(boxLeft + 2, centerY + 2, boxWidth - 4, 1),
            Text = "Press SPACE to increment, ESC to return to menu",
            Alignment = TextAlignment.Center,
            ForegroundColor = Color.DarkGray,
        };
        app.AddWidget(statusLabel);

        int btnWidth = (boxWidth - 8) / 2;
        var incrementButton = new Button
        {
            Bounds = new Rectangle(boxLeft + 2, centerY + 4, btnWidth, 1),
            Text = "Increment",
            IsFocused = true,
            InvokeKey = ConsoleKey.Spacebar,
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
                statusLabel.Text = "Complete! Press ESC to return to menu";
                statusLabel.ForegroundColor = Color.Green;
            }
        };
        app.AddWidget(incrementButton);

        var closeButton = new Button
        {
            Bounds = new Rectangle(boxLeft + 4 + btnWidth, centerY + 4, btnWidth, 1),
            Text = "Close",
        };
        closeButton.Click += (s, e) => app.Stop();
        app.AddWidget(closeButton);

        app.Run();
    }
}
