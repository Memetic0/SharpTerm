using SharpTerm.Core;
using SharpTerm.Core.Animation;
using SharpTerm.Core.Commands;
using SharpTerm.Core.DataBinding;
using SharpTerm.Core.Events;
using SharpTerm.Core.Focus;
using SharpTerm.Core.Input;
using SharpTerm.Core.Layout;
using SharpTerm.Core.Platform;
using SharpTerm.Core.Rendering;
using SharpTerm.Core.State;
using SharpTerm.Core.Widgets;

namespace SharpTerm.Samples.Demos.ArchitectureDemo;

/// <summary>
/// Demonstrates all new architecture features in SharpTerm.
/// </summary>
public static class ArchitectureFeatures
{
    public static void Run()
    {
        var app = new Application();

        // Display terminal capabilities
        DisplayTerminalCapabilities();

        // Setup focus manager with spatial navigation
        var focusManager = SetupFocusManager();

        // Setup command manager for undo/redo
        var commandManager = new CommandManager();

        // Setup state management
        var store = SetupStateManagement();

        // Setup animation manager
        var animationManager = new AnimationManager();

        // Create demo widgets
        var title = new Label
        {
            Text = "SharpTerm Architecture Demo",
            Bounds = new Rectangle(2, 1, 60, 1),
            ForegroundColor = Color.Cyan
        };

        var capabilitiesLabel = new Label
        {
            Text = GetCapabilitiesText(),
            Bounds = new Rectangle(2, 3, 76, 8),
            ForegroundColor = Color.White
        };

        var commandDemo = CreateCommandDemo(commandManager);
        commandDemo.Bounds = new Rectangle(2, 12, 36, 5);

        var stateDemo = CreateStateDemo(store);
        stateDemo.Bounds = new Rectangle(40, 12, 36, 5);

        var animationDemo = CreateAnimationDemo(animationManager);
        animationDemo.Bounds = new Rectangle(2, 18, 36, 5);

        var focusDemo = CreateFocusDemo(focusManager);
        focusDemo.Bounds = new Rectangle(40, 18, 36, 5);

        var instructions = new Label
        {
            Text = "Tab: Change focus | Space: Animate | U: Undo | R: Redo | ESC: Exit",
            Bounds = new Rectangle(2, 24, 76, 1),
            ForegroundColor = Color.Yellow
        };

        // Add all widgets
        app.AddWidget(title);
        app.AddWidget(capabilitiesLabel);
        app.AddWidget(commandDemo);
        app.AddWidget(stateDemo);
        app.AddWidget(animationDemo);
        app.AddWidget(focusDemo);
        app.AddWidget(instructions);

        // Register focusable widgets
        focusManager.RegisterFocusable(commandDemo);
        focusManager.RegisterFocusable(stateDemo);
        focusManager.RegisterFocusable(animationDemo);
        focusManager.RegisterFocusable(focusDemo);

        // Set initial focus
        focusManager.SetFocus(commandDemo);

        // Handle keyboard shortcuts
        var inputHandler = new ShortcutHandler();
        inputHandler.RegisterShortcut(ConsoleKey.Tab, ConsoleModifiers.None, ctx => focusManager.FocusNext());
        inputHandler.RegisterShortcut(ConsoleKey.U, ConsoleModifiers.None, ctx => commandManager.Undo());
        inputHandler.RegisterShortcut(ConsoleKey.R, ConsoleModifiers.None, ctx => commandManager.Redo());

        app.Run();
    }

    private static void DisplayTerminalCapabilities()
    {
        var caps = TerminalCapabilities.Current;
        Console.WriteLine("Terminal Capabilities Detected:");
        Console.WriteLine($"  Terminal Type: {caps.TerminalType}");
        Console.WriteLine($"  True Color: {caps.SupportsTrueColor}");
        Console.WriteLine($"  Mouse Support: {caps.SupportsMouseInput}");
        Console.WriteLine($"  Unicode: {caps.SupportsUnicode}");
        Console.WriteLine($"  Max Colors: {caps.MaxColors}");
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }

    private static string GetCapabilitiesText()
    {
        var caps = TerminalCapabilities.Current;
        return $@"Terminal Capabilities:
  Type: {caps.TerminalType}
  Colors: {caps.MaxColors} ({caps.GetBestColorMode()})
  True Color: {(caps.SupportsTrueColor ? "Yes" : "No")}
  Mouse: {(caps.SupportsMouseInput ? "Yes" : "No")}
  Unicode: {(caps.SupportsUnicode ? "Yes" : "No")}
  Hyperlinks: {(caps.SupportsHyperlinks ? "Yes" : "No")}";
    }

    private static IFocusManager SetupFocusManager()
    {
        var manager = new FocusManager
        {
            NavigationStrategy = new SpatialNavigationStrategy()
        };

        manager.FocusChanged += (s, e) =>
        {
            Console.Title = $"Focus: {e.NewFocus?.GetType().Name ?? "None"}";
        };

        return manager;
    }

    private static Store<AppState> SetupStateManagement()
    {
        var initialState = new AppState
        {
            CurrentView = "main",
            IsLoading = false
        };

        var store = new Store<AppState>(initialState, AppStateReducer.Reduce);

        // Add logging middleware
        store.AddMiddleware(CommonMiddleware.Logger<AppState>(msg =>
        {
            // Could log to file in production
        }));

        return store;
    }

    private static Border CreateCommandDemo(CommandManager commandManager)
    {
        var border = new Border
        {
            Title = "Command Pattern",
            Style = BorderStyle.Single
        };

        var label = new Label
        {
            Text = "Commands: 0",
            Bounds = new Rectangle(4, 14, 32, 1),
            ForegroundColor = Color.Green
        };

        var counter = 0;
        var incrementCommand = new DelegateCommand(
            "Increment",
            () =>
            {
                counter++;
                label.Text = $"Commands: {counter}";
            },
            () => true,
            () =>
            {
                counter--;
                label.Text = $"Commands: {counter}";
            }
        );

        border.AddChild(label);

        return border;
    }

    private static Border CreateStateDemo(Store<AppState> store)
    {
        var border = new Border
        {
            Title = "State Management",
            Style = BorderStyle.Single
        };

        var label = new Label
        {
            Bounds = new Rectangle(42, 14, 32, 1),
            ForegroundColor = Color.Magenta
        };

        // Subscribe to state changes
        store.Subscribe(state =>
        {
            label.Text = $"View: {state.CurrentView}";
        });

        // Initial render
        label.Text = $"View: {store.State.CurrentView}";

        border.AddChild(label);

        return border;
    }

    private static Border CreateAnimationDemo(AnimationManager animationManager)
    {
        var border = new Border
        {
            Title = "Animation Framework",
            Style = BorderStyle.Single,
            ForegroundColor = Color.White
        };

        var button = new Button
        {
            Text = "Animate Me!",
            Bounds = new Rectangle(6, 20, 28, 3),
            ForegroundColor = Color.White,
            Style = ButtonStyle.Rounded
        };

        button.Click += (s, e) =>
        {
            // Create color animation
            var anim = button.AnimateForegroundColor(
                Color.Red,
                1000,
                Easing.EaseInOutQuad
            );

            anim.Completed += (sender, args) =>
            {
                // Animate back
                var backAnim = button.AnimateForegroundColor(
                    Color.White,
                    1000,
                    Easing.EaseInOutQuad
                );
                animationManager.Add(backAnim);
                backAnim.Start();
            };

            animationManager.Add(anim);
            anim.Start();
        };

        border.AddChild(button);

        return border;
    }

    private static Border CreateFocusDemo(IFocusManager focusManager)
    {
        var border = new Border
        {
            Title = "Focus Management",
            Style = BorderStyle.Double
        };

        var label = new Label
        {
            Text = "Tab to navigate",
            Bounds = new Rectangle(44, 20, 28, 1),
            ForegroundColor = Color.Cyan
        };

        border.AddChild(label);

        return border;
    }
}
