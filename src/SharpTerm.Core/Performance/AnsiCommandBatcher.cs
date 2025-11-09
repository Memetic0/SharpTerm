using System.Text;

namespace SharpTerm.Core.Performance;

/// <summary>
/// Batches ANSI escape sequences to reduce Console.Write() calls.
/// </summary>
public class AnsiCommandBatcher
{
    private readonly StringBuilder _buffer;
    private Color? _currentForeground;
    private Color? _currentBackground;
    private (int x, int y)? _currentPosition;
    private int _pendingCommands;
    private const int FlushThreshold = 50; // Flush after this many commands

    public AnsiCommandBatcher(int initialCapacity = 4096)
    {
        _buffer = new StringBuilder(initialCapacity);
    }

    /// <summary>
    /// Sets the cursor position (batched).
    /// </summary>
    public void SetCursorPosition(int x, int y)
    {
        // Only emit if position actually changed
        if (_currentPosition.HasValue && _currentPosition.Value == (x, y))
            return;

        _buffer.Append($"\x1b[{y + 1};{x + 1}H");
        _currentPosition = (x, y);
        _pendingCommands++;

        if (_pendingCommands >= FlushThreshold)
            AutoFlush();
    }

    /// <summary>
    /// Sets foreground color (batched).
    /// </summary>
    public void SetForegroundColor(Color color)
    {
        if (_currentForeground.HasValue && ColorEquals(_currentForeground.Value, color))
            return;

        _buffer.Append($"\x1b[38;2;{color.R};{color.G};{color.B}m");
        _currentForeground = color;
        _pendingCommands++;

        if (_pendingCommands >= FlushThreshold)
            AutoFlush();
    }

    /// <summary>
    /// Sets background color (batched).
    /// </summary>
    public void SetBackgroundColor(Color color)
    {
        if (_currentBackground.HasValue && ColorEquals(_currentBackground.Value, color))
            return;

        if (color.R == 0 && color.G == 0 && color.B == 1) // Transparent
        {
            _buffer.Append("\x1b[49m"); // Default background
        }
        else
        {
            _buffer.Append($"\x1b[48;2;{color.R};{color.G};{color.B}m");
        }

        _currentBackground = color;
        _pendingCommands++;

        if (_pendingCommands >= FlushThreshold)
            AutoFlush();
    }

    /// <summary>
    /// Writes text (batched).
    /// </summary>
    public void Write(string text)
    {
        _buffer.Append(text);

        // Update virtual cursor position
        if (_currentPosition.HasValue)
        {
            _currentPosition = (_currentPosition.Value.x + text.Length, _currentPosition.Value.y);
        }
    }

    /// <summary>
    /// Writes text with colors (batched).
    /// </summary>
    public void Write(string text, Color foreground, Color background)
    {
        SetForegroundColor(foreground);
        SetBackgroundColor(background);
        Write(text);
    }

    /// <summary>
    /// Writes text at position with colors (batched).
    /// </summary>
    public void WriteAt(int x, int y, string text, Color foreground, Color background)
    {
        SetCursorPosition(x, y);
        SetForegroundColor(foreground);
        SetBackgroundColor(background);
        Write(text);
    }

    /// <summary>
    /// Clears the screen (batched).
    /// </summary>
    public void Clear()
    {
        _buffer.Append("\x1b[2J");
        _currentPosition = null;
        _pendingCommands++;
    }

    /// <summary>
    /// Resets all attributes (batched).
    /// </summary>
    public void Reset()
    {
        _buffer.Append("\x1b[0m");
        _currentForeground = null;
        _currentBackground = null;
        _pendingCommands++;
    }

    /// <summary>
    /// Flushes all batched commands to the console.
    /// </summary>
    public void Flush()
    {
        if (_buffer.Length > 0)
        {
            Console.Write(_buffer.ToString());
            _buffer.Clear();
            _pendingCommands = 0;
        }
    }

    /// <summary>
    /// Gets the current buffer without flushing.
    /// </summary>
    public string GetBuffer()
    {
        return _buffer.ToString();
    }

    /// <summary>
    /// Gets the number of pending commands.
    /// </summary>
    public int PendingCommandCount => _pendingCommands;

    /// <summary>
    /// Gets the current buffer size in characters.
    /// </summary>
    public int BufferSize => _buffer.Length;

    private void AutoFlush()
    {
        // Auto-flush when buffer gets too large
        if (_buffer.Length > 16384) // 16KB
        {
            Flush();
        }
    }

    private static bool ColorEquals(Color a, Color b)
    {
        return a.R == b.R && a.G == b.G && a.B == b.B;
    }
}

/// <summary>
/// Optimized ANSI sequence builder with command coalescing.
/// </summary>
public class AnsiSequenceOptimizer
{
    /// <summary>
    /// Combines multiple sequential ANSI commands into optimized sequences.
    /// </summary>
    public static string OptimizeSequence(string input)
    {
        var result = new StringBuilder(input.Length);
        var i = 0;

        while (i < input.Length)
        {
            if (input[i] == '\x1b' && i + 1 < input.Length && input[i + 1] == '[')
            {
                // Found ANSI sequence, collect consecutive sequences
                var sequences = new List<string>();
                while (i < input.Length && input[i] == '\x1b')
                {
                    var end = input.IndexOf('m', i);
                    if (end == -1)
                        end = input.IndexOf('H', i);
                    if (end == -1)
                        break;

                    sequences.Add(input.Substring(i, end - i + 1));
                    i = end + 1;

                    // Skip whitespace between sequences
                    while (i < input.Length && char.IsWhiteSpace(input[i]))
                        i++;
                }

                // Coalesce color sequences
                var optimized = CoalesceSequences(sequences);
                result.Append(optimized);
            }
            else
            {
                result.Append(input[i]);
                i++;
            }
        }

        return result.ToString();
    }

    private static string CoalesceSequences(List<string> sequences)
    {
        if (sequences.Count <= 1)
            return string.Join("", sequences);

        // Group by sequence type and keep only the last of each type
        var lastForeground = sequences.LastOrDefault(s => s.Contains("38;2"));
        var lastBackground = sequences.LastOrDefault(s => s.Contains("48;2"));
        var lastPosition = sequences.LastOrDefault(s => s.EndsWith('H'));
        var lastReset = sequences.LastOrDefault(s => s == "\x1b[0m");

        var result = new StringBuilder();
        if (lastReset != null) result.Append(lastReset);
        if (lastForeground != null) result.Append(lastForeground);
        if (lastBackground != null) result.Append(lastBackground);
        if (lastPosition != null) result.Append(lastPosition);

        return result.ToString();
    }
}
