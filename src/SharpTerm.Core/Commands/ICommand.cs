namespace SharpTerm.Core.Commands;

/// <summary>
/// Represents a command that can be executed, undone, and redone.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Determines whether the command can be executed.
    /// </summary>
    bool CanExecute();

    /// <summary>
    /// Executes the command.
    /// </summary>
    void Execute();

    /// <summary>
    /// Determines whether the command can be undone.
    /// </summary>
    bool CanUndo();

    /// <summary>
    /// Undoes the command.
    /// </summary>
    void Undo();

    /// <summary>
    /// Event raised when CanExecute changes.
    /// </summary>
    event EventHandler? CanExecuteChanged;
}

/// <summary>
/// Base class for commands with generic parameter.
/// </summary>
public abstract class Command<T> : ICommand
{
    protected T? Parameter { get; set; }

    public abstract string Name { get; }

    public virtual bool CanExecute() => true;

    public abstract void Execute();

    public virtual bool CanUndo() => false;

    public virtual void Undo()
    {
        throw new NotSupportedException($"Command '{Name}' does not support undo.");
    }

    public event EventHandler? CanExecuteChanged;

    protected void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetParameter(T parameter)
    {
        Parameter = parameter;
    }
}

/// <summary>
/// Command that uses delegates for execution.
/// </summary>
public class DelegateCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;
    private readonly Action? _undo;

    public string Name { get; }

    public DelegateCommand(string name, Action execute, Func<bool>? canExecute = null, Action? undo = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _undo = undo;
    }

    public bool CanExecute() => _canExecute?.Invoke() ?? true;

    public void Execute() => _execute();

    public bool CanUndo() => _undo != null;

    public void Undo()
    {
        if (_undo == null)
            throw new NotSupportedException($"Command '{Name}' does not support undo.");
        _undo();
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Command with a parameter.
/// </summary>
public class DelegateCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;
    private readonly Action<T>? _undo;
    private readonly T _parameter;

    public string Name { get; }

    public DelegateCommand(string name, Action<T> execute, T parameter, Func<T, bool>? canExecute = null, Action<T>? undo = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _parameter = parameter;
        _canExecute = canExecute;
        _undo = undo;
    }

    public bool CanExecute() => _canExecute?.Invoke(_parameter) ?? true;

    public void Execute() => _execute(_parameter);

    public bool CanUndo() => _undo != null;

    public void Undo()
    {
        if (_undo == null)
            throw new NotSupportedException($"Command '{Name}' does not support undo.");
        _undo(_parameter);
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
