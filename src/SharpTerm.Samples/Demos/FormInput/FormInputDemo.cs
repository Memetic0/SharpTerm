using SharpTerm.Core;
using SharpTerm.Core.Theming;
using SharpTerm.Core.Widgets;

namespace SharpTerm.Samples.Demos.FormInput;

public static class FormInputDemo
{
    public static void Run()
    {
        // Use new services and dependency injection
        var services = new Core.Services.ApplicationServices();
        var app = new Application(services.TerminalDriver);
        var themeManager = services.ThemeManager;

        // Use High Contrast theme for accessibility
        themeManager.SetTheme(Theme.HighContrast);

        int w = Console.WindowWidth;
        int h = Console.WindowHeight;
        int centerX = w / 2;
        int centerY = h / 2;
        int boxWidth = 60;
        int boxLeft = centerX - boxWidth / 2;

        // Border
        var border = new Border
        {
            Bounds = new Rectangle(boxLeft, centerY - 10, boxWidth, 20),
            Title = "User Registration Form",
            Style = BorderStyle.Double
        };

        // Title
        var titleLabel = new Label
        {
            Bounds = new Rectangle(boxLeft + 2, centerY - 8, boxWidth - 4, 1),
            Text = "Using Dependency Injection & Themes",
            Alignment = TextAlignment.Center
        };

        // Form fields
        var nameLabel = new Label
        {
            Bounds = new Rectangle(boxLeft + 2, centerY - 5, 16, 1),
            Text = "Name:",
            Alignment = TextAlignment.Right
        };

        var nameInput = new TextBox
        {
            Bounds = new Rectangle(boxLeft + 20, centerY - 5, 36, 1),
            Placeholder = "Enter your name",
            MaxLength = 50,
            IsFocused = true
        };

        var emailLabel = new Label
        {
            Bounds = new Rectangle(boxLeft + 2, centerY - 3, 16, 1),
            Text = "Email:",
            Alignment = TextAlignment.Right
        };

        var emailInput = new TextBox
        {
            Bounds = new Rectangle(boxLeft + 20, centerY - 3, 36, 1),
            Placeholder = "your@email.com",
            MaxLength = 50
        };

        var passwordLabel = new Label
        {
            Bounds = new Rectangle(boxLeft + 2, centerY - 1, 16, 1),
            Text = "Password:",
            Alignment = TextAlignment.Right
        };

        var passwordInput = new TextBox
        {
            Bounds = new Rectangle(boxLeft + 20, centerY - 1, 36, 1),
            Placeholder = "Enter password",
            PasswordChar = '*',
            MaxLength = 30
        };

        var resultLabel = new Label
        {
            Bounds = new Rectangle(boxLeft + 2, centerY + 2, boxWidth - 4, 2),
            Text = "Fill out the form and click Submit",
            Alignment = TextAlignment.Center
        };

        // Buttons
        var submitButton = new Button
        {
            Bounds = new Rectangle(boxLeft + 10, centerY + 6, 15, 1),
            Text = "Submit"
        };

        var cancelButton = new Button
        {
            Bounds = new Rectangle(boxLeft + 35, centerY + 6, 15, 1),
            Text = "Cancel"
        };

        // Apply theme
        themeManager.ApplyTheme(border);
        themeManager.ApplyTheme(titleLabel);
        themeManager.ApplyTheme(nameLabel);
        themeManager.ApplyTheme(nameInput);
        themeManager.ApplyTheme(emailLabel);
        themeManager.ApplyTheme(emailInput);
        themeManager.ApplyTheme(passwordLabel);
        themeManager.ApplyTheme(passwordInput);
        themeManager.ApplyTheme(resultLabel);
        themeManager.ApplyTheme(submitButton);
        themeManager.ApplyTheme(cancelButton);

        app.AddWidget(border);
        app.AddWidget(titleLabel);
        app.AddWidget(nameLabel);
        app.AddWidget(nameInput);
        app.AddWidget(emailLabel);
        app.AddWidget(emailInput);
        app.AddWidget(passwordLabel);
        app.AddWidget(passwordInput);
        app.AddWidget(resultLabel);
        app.AddWidget(submitButton);
        app.AddWidget(cancelButton);

        // Instructions
        var instructions = new Label
        {
            Bounds = new Rectangle(boxLeft, centerY + 12, boxWidth, 1),
            Text = "TAB to navigate | Enter to activate | ESC to exit",
            Alignment = TextAlignment.Center,
            ForegroundColor = Color.DarkGray
        };
        app.AddWidget(instructions);

        // Button handlers
        submitButton.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(nameInput.Text))
            {
                resultLabel.Text = "Error: Name is required!";
                resultLabel.ForegroundColor = Color.Red;
            }
            else if (string.IsNullOrWhiteSpace(emailInput.Text))
            {
                resultLabel.Text = "Error: Email is required!";
                resultLabel.ForegroundColor = Color.Red;
            }
            else if (passwordInput.Text.Length < 6)
            {
                resultLabel.Text = "Error: Password must be 6+ chars!";
                resultLabel.ForegroundColor = Color.Red;
            }
            else
            {
                resultLabel.Text = $"Success! Registered: {nameInput.Text}";
                resultLabel.ForegroundColor = Color.Green;
            }
        };

        cancelButton.Click += (s, e) => app.Stop();

        // Handle resize
        app.Resized += (s, e) =>
        {
            int newW = Console.WindowWidth;
            int newH = Console.WindowHeight;
            int newCenterX = newW / 2;
            int newCenterY = newH / 2;
            int newBoxLeft = newCenterX - boxWidth / 2;

            border.Bounds = new Rectangle(newBoxLeft, newCenterY - 10, boxWidth, 20);
            titleLabel.Bounds = new Rectangle(newBoxLeft + 2, newCenterY - 8, boxWidth - 4, 1);
            nameLabel.Bounds = new Rectangle(newBoxLeft + 2, newCenterY - 5, 16, 1);
            nameInput.Bounds = new Rectangle(newBoxLeft + 20, newCenterY - 5, 36, 1);
            emailLabel.Bounds = new Rectangle(newBoxLeft + 2, newCenterY - 3, 16, 1);
            emailInput.Bounds = new Rectangle(newBoxLeft + 20, newCenterY - 3, 36, 1);
            passwordLabel.Bounds = new Rectangle(newBoxLeft + 2, newCenterY - 1, 16, 1);
            passwordInput.Bounds = new Rectangle(newBoxLeft + 20, newCenterY - 1, 36, 1);
            resultLabel.Bounds = new Rectangle(newBoxLeft + 2, newCenterY + 2, boxWidth - 4, 2);
            submitButton.Bounds = new Rectangle(newBoxLeft + 10, newCenterY + 6, 15, 1);
            cancelButton.Bounds = new Rectangle(newBoxLeft + 35, newCenterY + 6, 15, 1);
            instructions.Bounds = new Rectangle(newBoxLeft, newCenterY + 12, boxWidth, 1);
            app.RequestRedraw();
        };

        app.Run();
    }
}
