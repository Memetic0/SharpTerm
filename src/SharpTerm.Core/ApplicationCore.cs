using SharpTerm.Core.ApplicationLogic;
using SharpTerm.Core.Performance;

namespace SharpTerm.Core;

/// <summary>
/// Main application class that manages the render loop and input handling.
/// </summary>
public class Application
{
    private readonly ITerminalDriver _driver;
    private readonly List<Widget> _widgets = new();
    private readonly LockFreeDirtyTracker _dirtyWidgets = new();
    private readonly SpatialIndex _spatialIndex;
    private readonly TerminalDimensionCache _dimensionCache = new();
    private readonly RenderFrameBudget _frameBudget = new(targetFps: 60);
    private readonly MouseEventOptimizer _mouseOptimizer = new();
    private bool _running;
    private int _focusedIndex = -1;
    private bool _needsFullRedraw = true;
    private bool _debugMode;
    private StreamWriter? _debugLogWriter;

    /// <summary>
    /// Event raised when the terminal is resized.
    /// </summary>
    public event EventHandler? Resized;

    public Application(ITerminalDriver? driver = null, bool debugMode = false)
    {
        _driver = driver ?? new AnsiTerminalDriver();
        _debugMode = debugMode;

        // Initialize spatial index with terminal bounds
        _spatialIndex = new SpatialIndex(_dimensionCache.GetBounds());

        if (_debugMode)
        {
            try
            {
                var logPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "sharpterm-debug.log"
                );
                _debugLogWriter = new StreamWriter(logPath, false) { AutoFlush = true };
                _debugLogWriter.WriteLine(
                    $"=== SharpTerm Debug Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==="
                );
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }

    private void DebugLog(string message)
    {
        if (_debugMode && _debugLogWriter != null)
        {
            try
            {
                _debugLogWriter.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }

    /// <summary>
    /// Adds a widget to the application.
    /// </summary>
    public void AddWidget(Widget widget)
    {
        widget.Changed += (s, e) =>
        {
            if (s is Widget w)
            {
                _dirtyWidgets.MarkDirty(w);
            }
        };
        _widgets.Add(widget);
        _spatialIndex.Insert(widget);
        _needsFullRedraw = true;
    }

    /// <summary>
    /// Removes a widget from the application.
    /// </summary>
    public void RemoveWidget(Widget widget)
    {
        _widgets.Remove(widget);
        _spatialIndex.Remove(widget);
        // No need to remove from dirty tracker - GetAndClearDirtyWidgets handles cleanup
        _needsFullRedraw = true;
    }

    /// <summary>
    /// Requests a full screen redraw on the next frame.
    /// </summary>
    public void RequestRedraw()
    {
        _needsFullRedraw = true;
    }

    /// <summary>
    /// Starts the application main loop.
    /// </summary>
    public void Run()
    {
        _running = true;
        _focusedIndex = FocusManager.InitializeFocus(_widgets);

        try
        {
            while (_running)
            {
                _frameBudget.BeginFrame();

                CheckForResize();

                if (_needsFullRedraw)
                {
                    Renderer.RenderAll(_driver, _widgets);
                    _needsFullRedraw = false;
                    _dirtyWidgets.GetAndClearDirtyWidgets();
                }
                else
                {
                    var dirtyWidgets = _dirtyWidgets.GetAndClearDirtyWidgets().ToList();
                    Renderer.RenderDirtyWidgets(_driver, dirtyWidgets);
                }

                bool shouldStop = InputProcessor.ProcessInput(
                    _driver,
                    _widgets,
                    _dirtyWidgets,
                    _spatialIndex,
                    _focusedIndex,
                    DebugLog,
                    Stop,
                    out _focusedIndex,
                    out bool needsRedraw
                );

                if (needsRedraw)
                {
                    _needsFullRedraw = true;
                }

                if (shouldStop)
                {
                    break;
                }

                _frameBudget.EndFrame();
            }
        }
        finally
        {
            _driver.Dispose();

            if (_debugLogWriter != null)
            {
                _debugLogWriter.WriteLine(
                    $"=== SharpTerm Debug Log Ended at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==="
                );
                _debugLogWriter.Dispose();
            }
        }
    }

    /// <summary>
    /// Stops the application.
    /// </summary>
    public void Stop() => _running = false;

    private void CheckForResize()
    {
        if (_dimensionCache.HasChanged())
        {
            _dimensionCache.ForceUpdate();
            var bounds = _dimensionCache.GetBounds();
            _needsFullRedraw = true;
            DebugLog($"Terminal resized to {bounds.Width}x{bounds.Height}");

            // Rebuild spatial index with new bounds
            _spatialIndex.Rebuild(_widgets);

            // Clear any pending input events to prevent spurious activations
            // Resize events can cause the input buffer to have stale data
            while (_driver.HasInputEvents)
            {
                _driver.ReadConsoleEvent(out _, out _);
            }

            Resized?.Invoke(this, EventArgs.Empty);
        }
    }
}
