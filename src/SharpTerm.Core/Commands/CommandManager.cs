namespace SharpTerm.Core.Commands;

/// <summary>
/// Manages command execution with undo/redo support.
/// </summary>
public class CommandManager
{
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();
    private int _maxHistorySize = 100;

    /// <summary>
    /// Gets or sets the maximum number of commands to keep in history.
    /// </summary>
    public int MaxHistorySize
    {
        get => _maxHistorySize;
        set => _maxHistorySize = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }

    /// <summary>
    /// Gets whether there are commands that can be undone.
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0 && _undoStack.Peek().CanUndo();

    /// <summary>
    /// Gets whether there are commands that can be redone.
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0 && _redoStack.Peek().CanExecute();

    /// <summary>
    /// Executes a command and adds it to the undo stack.
    /// </summary>
    public void Execute(ICommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (!command.CanExecute())
            return;

        command.Execute();

        if (command.CanUndo())
        {
            _undoStack.Push(command);
            _redoStack.Clear();

            // Trim history if needed
            while (_undoStack.Count > _maxHistorySize)
            {
                var items = _undoStack.ToArray();
                _undoStack.Clear();
                for (int i = 0; i < _maxHistorySize; i++)
                {
                    _undoStack.Push(items[i]);
                }
            }
        }
    }

    /// <summary>
    /// Undoes the last command.
    /// </summary>
    public void Undo()
    {
        if (!CanUndo)
            return;

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
    }

    /// <summary>
    /// Redoes the last undone command.
    /// </summary>
    public void Redo()
    {
        if (!CanRedo)
            return;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
    }

    /// <summary>
    /// Clears the command history.
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    /// <summary>
    /// Gets the command history (most recent first).
    /// </summary>
    public IEnumerable<ICommand> GetHistory()
    {
        return _undoStack.ToArray();
    }
}
