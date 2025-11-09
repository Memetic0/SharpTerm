using SharpTerm.Core;
using SharpTerm.Core.Animation;
using SharpTerm.Core.Commands;
using SharpTerm.Core.DataBinding;
using SharpTerm.Core.Focus;
using SharpTerm.Core.Layout;
using SharpTerm.Core.Platform;
using SharpTerm.Core.State;
using SharpTerm.Core.Widgets;

namespace SharpTerm.Samples.Demos.ArchitectureDemo;

/// <summary>
/// Simple demonstration of new architecture features.
/// </summary>
public static class SimpleArchitectureDemo
{
    public static void Run()
    {
        Console.Clear();
        Console.WriteLine("SharpTerm Architecture Improvements Demo");
        Console.WriteLine("=========================================\n");

        // 1. Terminal Capabilities Detection
        DemoTerminalCapabilities();

        // 2. Focus Management
        DemoFocusManagement();

        // 3. Command Pattern
        DemoCommandPattern();

        // 4. Data Binding
        DemoDataBinding();

        // 5. Animation Framework
        DemoAnimationFramework();

        // 6. State Management
        DemoStateManagement();

        // 7. Measurement/Arrangement
        DemoMeasureArrange();

        Console.WriteLine("\n\nDemo complete! Press any key to exit...");
        Console.ReadKey(true);
    }

    private static void DemoTerminalCapabilities()
    {
        Console.WriteLine("1. Terminal Capability Detection");
        Console.WriteLine("   -------------------------------");

        var caps = TerminalCapabilities.Current;

        Console.WriteLine($"   Terminal Type:    {caps.TerminalType}");
        Console.WriteLine($"   Color Mode:       {caps.GetBestColorMode()}");
        Console.WriteLine($"   Max Colors:       {caps.MaxColors}");
        Console.WriteLine($"   True Color:       {caps.SupportsTrueColor}");
        Console.WriteLine($"   256 Colors:       {caps.Supports256Colors}");
        Console.WriteLine($"   Mouse Support:    {caps.SupportsMouseInput}");
        Console.WriteLine($"   Unicode:          {caps.SupportsUnicode}");
        Console.WriteLine($"   Hyperlinks:       {caps.SupportsHyperlinks}");
        Console.WriteLine($"   Cursor Shapes:    {caps.SupportsCursorShapes}");
        Console.WriteLine($"   Bracketed Paste:  {caps.SupportsBracketedPaste}");

        // Demonstrate color degradation
        var original = new Color(128, 200, 255);
        var degraded = caps.DegradeColor(original);
        Console.WriteLine($"   Color Degrade:    RGB{original} -> RGB{degraded}");

        Console.WriteLine();
    }

    private static void DemoFocusManagement()
    {
        Console.WriteLine("2. Focus Management with Strategies");
        Console.WriteLine("   ----------------------------------");

        var focusManager = new FocusManager();

        // Create mock widgets
        var button1 = new Button { Text = "Button 1", Bounds = new Rectangle(0, 0, 10, 3) };
        var button2 = new Button { Text = "Button 2", Bounds = new Rectangle(0, 5, 10, 3) };
        var button3 = new Button { Text = "Button 3", Bounds = new Rectangle(15, 0, 10, 3) };

        focusManager.RegisterFocusable(button1);
        focusManager.RegisterFocusable(button2);
        focusManager.RegisterFocusable(button3);

        Console.WriteLine("   Tab Order Strategy:");
        focusManager.NavigationStrategy = new TabOrderNavigationStrategy();
        focusManager.SetFocus(button1);
        Console.WriteLine($"   - Current Focus: {((Button)focusManager.FocusedWidget!).Text}");
        focusManager.FocusNext();
        Console.WriteLine($"   - Next Focus:    {((Button)focusManager.FocusedWidget!).Text}");
        focusManager.FocusNext();
        Console.WriteLine($"   - Next Focus:    {((Button)focusManager.FocusedWidget!).Text}");

        Console.WriteLine("\n   Spatial Strategy:");
        focusManager.NavigationStrategy = new SpatialNavigationStrategy();
        focusManager.SetFocus(button1);
        Console.WriteLine($"   - Current Focus: {((Button)focusManager.FocusedWidget!).Text}");
        focusManager.FocusNext();
        Console.WriteLine($"   - Right/Down:    {((Button)focusManager.FocusedWidget!).Text}");

        Console.WriteLine();
    }

    private static void DemoCommandPattern()
    {
        Console.WriteLine("3. Command Pattern with Undo/Redo");
        Console.WriteLine("   --------------------------------");

        var commandManager = new CommandManager();
        var counter = 0;

        var incrementCommand = new DelegateCommand(
            "Increment",
            () => counter++,
            () => true,
            () => counter--
        );

        Console.WriteLine($"   Initial value:    {counter}");

        commandManager.Execute(incrementCommand);
        Console.WriteLine($"   After increment:  {counter}");

        commandManager.Execute(incrementCommand);
        Console.WriteLine($"   After increment:  {counter}");

        commandManager.Execute(incrementCommand);
        Console.WriteLine($"   After increment:  {counter}");

        Console.WriteLine($"   Can Undo:         {commandManager.CanUndo}");

        commandManager.Undo();
        Console.WriteLine($"   After undo:       {counter}");

        commandManager.Undo();
        Console.WriteLine($"   After undo:       {counter}");

        Console.WriteLine($"   Can Redo:         {commandManager.CanRedo}");

        commandManager.Redo();
        Console.WriteLine($"   After redo:       {counter}");

        Console.WriteLine();
    }

    private static void DemoDataBinding()
    {
        Console.WriteLine("4. Data Binding Framework");
        Console.WriteLine("   -----------------------");

        // Create a model
        var model = new PersonModel { Name = "John", Age = 30 };

        // Create a label widget
        var label = new Label { Bounds = new Rectangle(0, 0, 20, 1) };

        // Bind label text to model name
        var bindingManager = new BindingManager();
        bindingManager.Bind(label, "Text", model, "Name");

        Console.WriteLine($"   Initial Label:    '{label.Text}'");

        model.Name = "Jane";
        Console.WriteLine($"   After Change:     '{label.Text}'");

        model.Name = "Bob";
        Console.WriteLine($"   After Change:     '{label.Text}'");

        bindingManager.Dispose();
        Console.WriteLine();
    }

    private static void DemoAnimationFramework()
    {
        Console.WriteLine("5. Animation Framework with Easing");
        Console.WriteLine("   ---------------------------------");

        var animManager = new AnimationManager();

        // Test easing functions
        Console.WriteLine("   Easing Functions:");
        var t = 0.5;
        Console.WriteLine($"   - Linear(0.5):      {Easing.Linear.Ease(t):F3}");
        Console.WriteLine($"   - EaseInQuad(0.5):  {Easing.EaseInQuad.Ease(t):F3}");
        Console.WriteLine($"   - EaseOutQuad(0.5): {Easing.EaseOutQuad.Ease(t):F3}");
        Console.WriteLine($"   - EaseInCubic(0.5): {Easing.EaseInCubic.Ease(t):F3}");

        // Test interpolators
        Console.WriteLine("\n   Interpolators:");
        var color1 = new Color(0, 0, 0);
        var color2 = new Color(255, 255, 255);
        var midColor = Interpolators.Interpolate(color1, color2, 0.5);
        Console.WriteLine($"   - Color: {color1} -> {color2} @ 0.5 = {midColor}");

        var rect1 = new Rectangle(0, 0, 10, 10);
        var rect2 = new Rectangle(20, 20, 30, 30);
        var midRect = Interpolators.Interpolate(rect1, rect2, 0.5);
        Console.WriteLine($"   - Rectangle: {rect1} -> {rect2}");
        Console.WriteLine($"                @ 0.5 = {midRect}");

        Console.WriteLine($"\n   Active Animations: {animManager.ActiveCount}");

        Console.WriteLine();
    }

    private static void DemoStateManagement()
    {
        Console.WriteLine("6. State Management (Redux Pattern)");
        Console.WriteLine("   ----------------------------------");

        var initialState = new AppState
        {
            CurrentView = "home",
            IsLoading = false
        };

        var store = new Store<AppState>(initialState, AppStateReducer.Reduce);

        Console.WriteLine($"   Initial State:    View={store.State.CurrentView}, Loading={store.State.IsLoading}");

        store.Dispatch(new NavigateAction("settings"));
        Console.WriteLine($"   After Navigate:   View={store.State.CurrentView}");

        store.Dispatch(new SetLoadingAction(true));
        Console.WriteLine($"   After SetLoading: Loading={store.State.IsLoading}");

        store.Dispatch(new SetDataAction("username", "Alice"));
        Console.WriteLine($"   After SetData:    Data count={store.State.Data.Count}");

        // Subscribe to changes
        var subscription = store.Subscribe(state =>
        {
            Console.WriteLine($"   [Subscription]    View changed to: {state.CurrentView}");
        });

        store.Dispatch(new NavigateAction("profile"));

        subscription.Dispose();
        Console.WriteLine();
    }

    private static void DemoMeasureArrange()
    {
        Console.WriteLine("7. Measurement/Arrangement System");
        Console.WriteLine("   --------------------------------");

        var widget = new TestMeasurableWidget
        {
            Margin = new Thickness(5, 2),
            Padding = new Thickness(3),
            MinWidth = 20,
            MinHeight = 10
        };

        var availableSize = new Size(100, 50);
        Console.WriteLine($"   Available Size:   {availableSize}");

        var desiredSize = widget.Measure(availableSize);
        Console.WriteLine($"   Desired Size:     {desiredSize}");

        var finalRect = new Rectangle(0, 0, 100, 50);
        widget.Arrange(finalRect);
        Console.WriteLine($"   Final Bounds:     {widget.Bounds}");

        Console.WriteLine();
    }

    // Helper classes
    private class PersonModel : ObservableObject
    {
        private string _name = "";
        private int _age;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Age
        {
            get => _age;
            set => SetProperty(ref _age, value);
        }
    }

    private class TestMeasurableWidget : MeasurableWidget
    {
        public override void Render(ITerminalDriver driver)
        {
            // No rendering needed for this demo
        }

        protected override Size MeasureCore(Size availableSize)
        {
            return new Size(30, 15);
        }
    }
}
