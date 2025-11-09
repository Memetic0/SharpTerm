using System.Reflection;

namespace SharpTerm.Core.DataBinding;

/// <summary>
/// Represents a binding between a widget property and a data source property.
/// </summary>
public class Binding : IDisposable
{
    private readonly Widget _target;
    private readonly string _targetProperty;
    private readonly object _source;
    private readonly string _sourceProperty;
    private readonly BindingMode _mode;
    private readonly IValueConverter? _converter;
    private bool _updating;

    public Binding(Widget target, string targetProperty, object source, string sourceProperty,
        BindingMode mode = BindingMode.OneWay, IValueConverter? converter = null)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _targetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _sourceProperty = sourceProperty ?? throw new ArgumentNullException(nameof(sourceProperty));
        _mode = mode;
        _converter = converter;

        // Subscribe to source property changes
        if (_source is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnSourcePropertyChanged;
        }

        // Subscribe to target property changes for TwoWay binding
        if (_mode == BindingMode.TwoWay)
        {
            // Note: This would require widgets to implement INotifyPropertyChanged
            // For now, we'll support this through widget events
        }

        // Initial update
        UpdateTarget();
    }

    private void OnSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _sourceProperty && !_updating)
        {
            UpdateTarget();
        }
    }

    private void UpdateTarget()
    {
        if (_updating) return;
        _updating = true;

        try
        {
            var sourceValue = GetPropertyValue(_source, _sourceProperty);
            if (_converter != null)
            {
                sourceValue = _converter.Convert(sourceValue);
            }

            SetPropertyValue(_target, _targetProperty, sourceValue);
        }
        finally
        {
            _updating = false;
        }
    }

    private void UpdateSource()
    {
        if (_updating || _mode != BindingMode.TwoWay) return;
        _updating = true;

        try
        {
            var targetValue = GetPropertyValue(_target, _targetProperty);
            if (_converter != null)
            {
                targetValue = _converter.ConvertBack(targetValue);
            }

            SetPropertyValue(_source, _sourceProperty, targetValue);
        }
        finally
        {
            _updating = false;
        }
    }

    private object? GetPropertyValue(object obj, string propertyName)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return prop?.GetValue(obj);
    }

    private void SetPropertyValue(object obj, string propertyName, object? value)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(obj, value);
        }
    }

    public void Dispose()
    {
        if (_source is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= OnSourcePropertyChanged;
        }
    }
}

/// <summary>
/// Specifies the direction of data flow in a binding.
/// </summary>
public enum BindingMode
{
    /// <summary>
    /// Updates the target property when the source property changes.
    /// </summary>
    OneWay,

    /// <summary>
    /// Updates both the target and source properties when either changes.
    /// </summary>
    TwoWay,

    /// <summary>
    /// Updates the target property only once when the binding is created.
    /// </summary>
    OneTime
}

/// <summary>
/// Converts values between source and target in bindings.
/// </summary>
public interface IValueConverter
{
    /// <summary>
    /// Converts a value from source to target.
    /// </summary>
    object? Convert(object? value);

    /// <summary>
    /// Converts a value from target to source.
    /// </summary>
    object? ConvertBack(object? value);
}

/// <summary>
/// Manages bindings for a widget.
/// </summary>
public class BindingManager : IDisposable
{
    private readonly List<Binding> _bindings = new();

    /// <summary>
    /// Creates and adds a binding.
    /// </summary>
    public Binding Bind(Widget target, string targetProperty, object source, string sourceProperty,
        BindingMode mode = BindingMode.OneWay, IValueConverter? converter = null)
    {
        var binding = new Binding(target, targetProperty, source, sourceProperty, mode, converter);
        _bindings.Add(binding);
        return binding;
    }

    /// <summary>
    /// Removes a binding.
    /// </summary>
    public void Unbind(Binding binding)
    {
        if (_bindings.Remove(binding))
        {
            binding.Dispose();
        }
    }

    /// <summary>
    /// Clears all bindings.
    /// </summary>
    public void Clear()
    {
        foreach (var binding in _bindings)
        {
            binding.Dispose();
        }
        _bindings.Clear();
    }

    public void Dispose()
    {
        Clear();
    }
}
