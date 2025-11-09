namespace SharpTerm.Core.DataBinding;

/// <summary>
/// Notifies clients that a property value has changed.
/// </summary>
public interface INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// Represents the method that will handle the PropertyChanged event.
/// </summary>
public delegate void PropertyChangedEventHandler(object? sender, PropertyChangedEventArgs e);

/// <summary>
/// Provides data for the PropertyChanged event.
/// </summary>
public class PropertyChangedEventArgs : EventArgs
{
    public string PropertyName { get; }

    public PropertyChangedEventArgs(string propertyName)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
    }
}

/// <summary>
/// Base class for objects that implement INotifyPropertyChanged.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets the property value and raises PropertyChanged if the value changed.
    /// </summary>
    /// <returns>True if the value was changed.</returns>
    protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
