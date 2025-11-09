namespace SharpTerm.Core.Animation;

/// <summary>
/// Represents an animation that can be updated over time.
/// </summary>
public interface IAnimation
{
    /// <summary>
    /// Gets whether the animation is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets whether the animation has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the duration of the animation in milliseconds.
    /// </summary>
    double Duration { get; }

    /// <summary>
    /// Gets the current time position in milliseconds.
    /// </summary>
    double CurrentTime { get; }

    /// <summary>
    /// Starts the animation.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the animation.
    /// </summary>
    void Stop();

    /// <summary>
    /// Pauses the animation.
    /// </summary>
    void Pause();

    /// <summary>
    /// Resumes the animation.
    /// </summary>
    void Resume();

    /// <summary>
    /// Updates the animation with the elapsed time.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in milliseconds.</param>
    void Update(double deltaTime);

    /// <summary>
    /// Event raised when the animation completes.
    /// </summary>
    event EventHandler? Completed;
}

/// <summary>
/// Base class for animations.
/// </summary>
public abstract class Animation : IAnimation
{
    private double _currentTime;
    private bool _isRunning;
    private bool _isPaused;

    public bool IsRunning => _isRunning && !_isPaused;
    public bool IsCompleted { get; private set; }
    public double Duration { get; protected set; }
    public double CurrentTime => _currentTime;

    public event EventHandler? Completed;

    public virtual void Start()
    {
        _isRunning = true;
        _isPaused = false;
        _currentTime = 0;
        IsCompleted = false;
    }

    public virtual void Stop()
    {
        _isRunning = false;
        _isPaused = false;
        _currentTime = 0;
        IsCompleted = true;
    }

    public virtual void Pause()
    {
        _isPaused = true;
    }

    public virtual void Resume()
    {
        _isPaused = false;
    }

    public virtual void Update(double deltaTime)
    {
        if (!IsRunning)
            return;

        _currentTime += deltaTime;

        if (_currentTime >= Duration)
        {
            _currentTime = Duration;
            ApplyAnimation(1.0);
            Complete();
        }
        else
        {
            var progress = _currentTime / Duration;
            ApplyAnimation(progress);
        }
    }

    protected abstract void ApplyAnimation(double progress);

    protected virtual void Complete()
    {
        IsCompleted = true;
        _isRunning = false;
        Completed?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Easing functions for animations.
/// </summary>
public interface IEasingFunction
{
    double Ease(double t);
}

/// <summary>
/// Common easing functions.
/// </summary>
public static class Easing
{
    public static readonly IEasingFunction Linear = new LinearEasing();
    public static readonly IEasingFunction EaseInQuad = new QuadraticEaseIn();
    public static readonly IEasingFunction EaseOutQuad = new QuadraticEaseOut();
    public static readonly IEasingFunction EaseInOutQuad = new QuadraticEaseInOut();
    public static readonly IEasingFunction EaseInCubic = new CubicEaseIn();
    public static readonly IEasingFunction EaseOutCubic = new CubicEaseOut();
    public static readonly IEasingFunction EaseInOutCubic = new CubicEaseInOut();

    private class LinearEasing : IEasingFunction
    {
        public double Ease(double t) => t;
    }

    private class QuadraticEaseIn : IEasingFunction
    {
        public double Ease(double t) => t * t;
    }

    private class QuadraticEaseOut : IEasingFunction
    {
        public double Ease(double t) => t * (2 - t);
    }

    private class QuadraticEaseInOut : IEasingFunction
    {
        public double Ease(double t) => t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    private class CubicEaseIn : IEasingFunction
    {
        public double Ease(double t) => t * t * t;
    }

    private class CubicEaseOut : IEasingFunction
    {
        public double Ease(double t) => (--t) * t * t + 1;
    }

    private class CubicEaseInOut : IEasingFunction
    {
        public double Ease(double t) => t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
    }
}

/// <summary>
/// Animates a property value from start to end.
/// </summary>
public class ValueAnimation<T> : Animation where T : struct
{
    private readonly Action<T> _setter;
    private readonly Func<T, T, double, T> _interpolator;
    private readonly IEasingFunction _easing;

    public T FromValue { get; set; }
    public T ToValue { get; set; }

    public ValueAnimation(
        T from,
        T to,
        double duration,
        Action<T> setter,
        Func<T, T, double, T> interpolator,
        IEasingFunction? easing = null)
    {
        FromValue = from;
        ToValue = to;
        Duration = duration;
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
        _interpolator = interpolator ?? throw new ArgumentNullException(nameof(interpolator));
        _easing = easing ?? Easing.Linear;
    }

    protected override void ApplyAnimation(double progress)
    {
        var easedProgress = _easing.Ease(progress);
        var value = _interpolator(FromValue, ToValue, easedProgress);
        _setter(value);
    }
}

/// <summary>
/// Common interpolators for different types.
/// </summary>
public static class Interpolators
{
    public static int Interpolate(int from, int to, double t)
    {
        return (int)(from + (to - from) * t);
    }

    public static double Interpolate(double from, double to, double t)
    {
        return from + (to - from) * t;
    }

    public static Color Interpolate(Color from, Color to, double t)
    {
        var r = (byte)(from.R + (to.R - from.R) * t);
        var g = (byte)(from.G + (to.G - from.G) * t);
        var b = (byte)(from.B + (to.B - from.B) * t);
        return new Color(r, g, b);
    }

    public static Rectangle Interpolate(Rectangle from, Rectangle to, double t)
    {
        return new Rectangle(
            Interpolate(from.X, to.X, t),
            Interpolate(from.Y, to.Y, t),
            Interpolate(from.Width, to.Width, t),
            Interpolate(from.Height, to.Height, t)
        );
    }
}
