using System.Diagnostics;

namespace SharpTerm.Core.Performance;

/// <summary>
/// Manages rendering with a time budget per frame (e.g., 16ms for 60 FPS).
/// </summary>
public class RenderFrameBudget
{
    private readonly Stopwatch _frameTimer = new();
    private readonly TimeSpan _frameBudget;
    private readonly Queue<Action> _deferredOperations = new();

    public RenderFrameBudget(double targetFps = 60.0)
    {
        _frameBudget = TimeSpan.FromMilliseconds(1000.0 / targetFps);
    }

    /// <summary>
    /// Gets the target frame budget.
    /// </summary>
    public TimeSpan FrameBudget => _frameBudget;

    /// <summary>
    /// Gets the elapsed time in the current frame.
    /// </summary>
    public TimeSpan ElapsedThisFrame => _frameTimer.Elapsed;

    /// <summary>
    /// Gets the remaining time in the current frame budget.
    /// </summary>
    public TimeSpan RemainingTime => _frameBudget - ElapsedThisFrame;

    /// <summary>
    /// Checks if there's enough time left in the frame budget.
    /// </summary>
    public bool HasTimeRemaining => ElapsedThisFrame < _frameBudget;

    /// <summary>
    /// Checks if a specific amount of time is available.
    /// </summary>
    public bool HasTime(TimeSpan required) => ElapsedThisFrame + required < _frameBudget;

    /// <summary>
    /// Starts timing a new frame.
    /// </summary>
    public void BeginFrame()
    {
        _frameTimer.Restart();
    }

    /// <summary>
    /// Ends the current frame and sleeps if needed to maintain target FPS.
    /// </summary>
    public void EndFrame()
    {
        _frameTimer.Stop();

        var frameTime = _frameTimer.Elapsed;
        if (frameTime < _frameBudget)
        {
            var sleepTime = _frameBudget - frameTime;
            Thread.Sleep(sleepTime);
        }
    }

    /// <summary>
    /// Executes an operation if there's time, otherwise defers it.
    /// </summary>
    public bool ExecuteOrDefer(Action operation, TimeSpan estimatedDuration)
    {
        if (HasTime(estimatedDuration))
        {
            operation();
            return true;
        }
        else
        {
            _deferredOperations.Enqueue(operation);
            return false;
        }
    }

    /// <summary>
    /// Processes deferred operations from previous frames.
    /// </summary>
    public void ProcessDeferredOperations()
    {
        while (_deferredOperations.Count > 0 && HasTimeRemaining)
        {
            var operation = _deferredOperations.Dequeue();
            operation();
        }
    }

    /// <summary>
    /// Gets the number of deferred operations.
    /// </summary>
    public int DeferredOperationCount => _deferredOperations.Count;

    /// <summary>
    /// Clears all deferred operations.
    /// </summary>
    public void ClearDeferred()
    {
        _deferredOperations.Clear();
    }

    /// <summary>
    /// Gets frame statistics.
    /// </summary>
    public FrameStatistics GetStatistics()
    {
        return new FrameStatistics(
            _frameTimer.Elapsed,
            _frameBudget,
            RemainingTime,
            _deferredOperations.Count,
            (double)_frameTimer.Elapsed.TotalMilliseconds / _frameBudget.TotalMilliseconds * 100
        );
    }
}

/// <summary>
/// Frame rendering statistics.
/// </summary>
public readonly struct FrameStatistics
{
    public TimeSpan FrameTime { get; }
    public TimeSpan FrameBudget { get; }
    public TimeSpan RemainingTime { get; }
    public int DeferredOperations { get; }
    public double BudgetUtilization { get; }

    public FrameStatistics(TimeSpan frameTime, TimeSpan frameBudget, TimeSpan remainingTime, int deferredOperations, double budgetUtilization)
    {
        FrameTime = frameTime;
        FrameBudget = frameBudget;
        RemainingTime = remainingTime;
        DeferredOperations = deferredOperations;
        BudgetUtilization = budgetUtilization;
    }

    public bool IsOverBudget => FrameTime > FrameBudget;

    public override string ToString() =>
        $"Frame: {FrameTime.TotalMilliseconds:F2}ms / {FrameBudget.TotalMilliseconds:F2}ms ({BudgetUtilization:F1}%), Deferred: {DeferredOperations}";
}

/// <summary>
/// Frame rate monitor for performance tracking.
/// </summary>
public class FrameRateMonitor
{
    private readonly Stopwatch _stopwatch = new();
    private readonly Queue<double> _frameTimes = new();
    private const int SampleSize = 60;
    private int _frameCount;

    public FrameRateMonitor()
    {
        _stopwatch.Start();
    }

    /// <summary>
    /// Records a frame completion.
    /// </summary>
    public void RecordFrame()
    {
        _frameCount++;
        var frameTime = _stopwatch.Elapsed.TotalMilliseconds;
        _frameTimes.Enqueue(frameTime);

        if (_frameTimes.Count > SampleSize)
        {
            _frameTimes.Dequeue();
        }

        _stopwatch.Restart();
    }

    /// <summary>
    /// Gets the current frames per second.
    /// </summary>
    public double CurrentFPS
    {
        get
        {
            if (_frameTimes.Count == 0)
                return 0;

            var avgFrameTime = _frameTimes.Average();
            return 1000.0 / avgFrameTime;
        }
    }

    /// <summary>
    /// Gets the minimum FPS in the recent sample.
    /// </summary>
    public double MinFPS
    {
        get
        {
            if (_frameTimes.Count == 0)
                return 0;

            var maxFrameTime = _frameTimes.Max();
            return 1000.0 / maxFrameTime;
        }
    }

    /// <summary>
    /// Gets the maximum FPS in the recent sample.
    /// </summary>
    public double MaxFPS
    {
        get
        {
            if (_frameTimes.Count == 0)
                return 0;

            var minFrameTime = _frameTimes.Min();
            return 1000.0 / minFrameTime;
        }
    }

    /// <summary>
    /// Gets the total number of frames rendered.
    /// </summary>
    public int TotalFrames => _frameCount;

    public override string ToString() =>
        $"FPS: {CurrentFPS:F1} (min: {MinFPS:F1}, max: {MaxFPS:F1}), Total Frames: {TotalFrames}";
}
