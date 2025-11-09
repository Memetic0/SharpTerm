using System.Diagnostics;

namespace SharpTerm.Core.Animation;

/// <summary>
/// Manages and updates all active animations.
/// </summary>
public class AnimationManager
{
    private readonly List<IAnimation> _animations = new();
    private readonly Stopwatch _stopwatch = new();
    private double _lastTime;

    public AnimationManager()
    {
        _stopwatch.Start();
    }

    /// <summary>
    /// Adds an animation to be managed.
    /// </summary>
    public void Add(IAnimation animation)
    {
        if (animation == null)
            throw new ArgumentNullException(nameof(animation));

        if (!_animations.Contains(animation))
        {
            _animations.Add(animation);
            animation.Completed += OnAnimationCompleted;
        }
    }

    /// <summary>
    /// Removes an animation.
    /// </summary>
    public void Remove(IAnimation animation)
    {
        if (_animations.Remove(animation))
        {
            animation.Completed -= OnAnimationCompleted;
        }
    }

    /// <summary>
    /// Updates all active animations.
    /// </summary>
    public void Update()
    {
        var currentTime = _stopwatch.Elapsed.TotalMilliseconds;
        var deltaTime = currentTime - _lastTime;
        _lastTime = currentTime;

        foreach (var animation in _animations.ToList())
        {
            if (animation.IsRunning)
            {
                animation.Update(deltaTime);
            }
        }
    }

    /// <summary>
    /// Clears all animations.
    /// </summary>
    public void Clear()
    {
        foreach (var animation in _animations.ToList())
        {
            animation.Stop();
            animation.Completed -= OnAnimationCompleted;
        }
        _animations.Clear();
    }

    /// <summary>
    /// Gets the number of active animations.
    /// </summary>
    public int ActiveCount => _animations.Count(a => a.IsRunning);

    private void OnAnimationCompleted(object? sender, EventArgs e)
    {
        if (sender is IAnimation animation)
        {
            Remove(animation);
        }
    }
}

/// <summary>
/// Helper methods for creating common animations.
/// </summary>
public static class AnimationExtensions
{
    /// <summary>
    /// Animates a widget's bounds.
    /// </summary>
    public static ValueAnimation<Rectangle> AnimateBounds(
        this Widget widget,
        Rectangle to,
        double duration,
        IEasingFunction? easing = null)
    {
        var animation = new ValueAnimation<Rectangle>(
            widget.Bounds,
            to,
            duration,
            value =>
            {
                widget.Bounds = value;
            },
            Interpolators.Interpolate,
            easing
        );

        return animation;
    }

    /// <summary>
    /// Animates a widget's foreground color.
    /// </summary>
    public static ValueAnimation<Color> AnimateForegroundColor(
        this Widget widget,
        Color to,
        double duration,
        IEasingFunction? easing = null)
    {
        var animation = new ValueAnimation<Color>(
            widget.ForegroundColor,
            to,
            duration,
            value =>
            {
                widget.ForegroundColor = value;
            },
            Interpolators.Interpolate,
            easing
        );

        return animation;
    }

    /// <summary>
    /// Animates a widget's background color.
    /// </summary>
    public static ValueAnimation<Color> AnimateBackgroundColor(
        this Widget widget,
        Color to,
        double duration,
        IEasingFunction? easing = null)
    {
        var animation = new ValueAnimation<Color>(
            widget.BackgroundColor,
            to,
            duration,
            value =>
            {
                widget.BackgroundColor = value;
            },
            Interpolators.Interpolate,
            easing
        );

        return animation;
    }

    /// <summary>
    /// Fades a widget's color.
    /// </summary>
    public static ValueAnimation<Color> FadeColor(
        this Widget widget,
        Color from,
        Color to,
        double duration,
        IEasingFunction? easing = null)
    {
        return new ValueAnimation<Color>(
            from,
            to,
            duration,
            value => widget.ForegroundColor = value,
            Interpolators.Interpolate,
            easing
        );
    }
}

/// <summary>
/// Timeline for sequencing multiple animations.
/// </summary>
public class AnimationTimeline : Animation
{
    private readonly List<(double startTime, IAnimation animation)> _animations = new();
    private double _totalDuration;

    /// <summary>
    /// Adds an animation to start at the specified time.
    /// </summary>
    public void AddAnimation(double startTime, IAnimation animation)
    {
        _animations.Add((startTime, animation));
        _totalDuration = Math.Max(_totalDuration, startTime + animation.Duration);
        Duration = _totalDuration;
    }

    /// <summary>
    /// Adds an animation to start after the previous animation.
    /// </summary>
    public void AddAnimationSequential(IAnimation animation)
    {
        AddAnimation(_totalDuration, animation);
    }

    /// <summary>
    /// Adds an animation to start at the same time as the previous animation.
    /// </summary>
    public void AddAnimationParallel(IAnimation animation)
    {
        var lastStartTime = _animations.Any() ? _animations[^1].startTime : 0;
        AddAnimation(lastStartTime, animation);
    }

    public override void Start()
    {
        base.Start();
        foreach (var (_, animation) in _animations)
        {
            animation.Stop();
        }
    }

    protected override void ApplyAnimation(double progress)
    {
        var currentTime = progress * Duration;

        foreach (var (startTime, animation) in _animations)
        {
            var endTime = startTime + animation.Duration;

            if (currentTime >= startTime && currentTime <= endTime)
            {
                if (!animation.IsRunning && !animation.IsCompleted)
                {
                    animation.Start();
                }

                var animationTime = currentTime - startTime;
                var deltaTime = animationTime - animation.CurrentTime;
                animation.Update(deltaTime);
            }
        }
    }
}
