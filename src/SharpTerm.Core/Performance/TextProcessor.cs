namespace SharpTerm.Core.Performance;

/// <summary>
/// Zero-allocation text processing using ReadOnlySpan.
/// </summary>
public static class TextProcessor
{
    /// <summary>
    /// Splits text into lines without allocating strings.
    /// </summary>
    public static SpanLineEnumerator EnumerateLines(ReadOnlySpan<char> text)
    {
        return new SpanLineEnumerator(text);
    }

    /// <summary>
    /// Counts the number of lines in text.
    /// </summary>
    public static int CountLines(ReadOnlySpan<char> text)
    {
        int count = text.IsEmpty ? 0 : 1;
        foreach (var c in text)
        {
            if (c == '\n')
                count++;
        }
        return count;
    }

    /// <summary>
    /// Wraps text to fit within the specified width.
    /// </summary>
    public static List<string> WrapText(ReadOnlySpan<char> text, int width)
    {
        var lines = new List<string>();
        if (width <= 0 || text.IsEmpty)
            return lines;

        var currentLine = new System.Text.StringBuilder();
        int currentWidth = 0;

        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (c == '\n')
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
                currentWidth = 0;
                continue;
            }

            if (currentWidth >= width)
            {
                // Find last space for word wrap
                var lineStr = currentLine.ToString();
                var lastSpace = lineStr.LastIndexOf(' ');

                if (lastSpace > 0)
                {
                    lines.Add(lineStr.Substring(0, lastSpace));
                    currentLine.Clear();
                    currentLine.Append(lineStr.Substring(lastSpace + 1));
                    currentWidth = currentLine.Length;
                }
                else
                {
                    lines.Add(lineStr);
                    currentLine.Clear();
                    currentWidth = 0;
                }
            }

            currentLine.Append(c);
            currentWidth++;
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine.ToString());
        }

        return lines;
    }

    /// <summary>
    /// Truncates text to fit within width with ellipsis.
    /// </summary>
    public static string Truncate(ReadOnlySpan<char> text, int maxWidth, string ellipsis = "...")
    {
        if (text.Length <= maxWidth)
            return text.ToString();

        if (maxWidth < ellipsis.Length)
            return ellipsis.Substring(0, maxWidth);

        var truncated = text.Slice(0, maxWidth - ellipsis.Length);
        return truncated.ToString() + ellipsis;
    }

    /// <summary>
    /// Pads text to the specified width.
    /// </summary>
    public static string Pad(ReadOnlySpan<char> text, int width, char paddingChar = ' ', TextAlignment alignment = TextAlignment.Left)
    {
        if (text.Length >= width)
            return text.ToString();

        var padding = width - text.Length;

        return alignment switch
        {
            TextAlignment.Left => text.ToString() + new string(paddingChar, padding),
            TextAlignment.Right => new string(paddingChar, padding) + text.ToString(),
            TextAlignment.Center => new string(paddingChar, padding / 2) + text.ToString() + new string(paddingChar, padding - padding / 2),
            _ => text.ToString()
        };
    }

    /// <summary>
    /// Removes leading and trailing whitespace without allocation.
    /// </summary>
    public static ReadOnlySpan<char> Trim(ReadOnlySpan<char> text)
    {
        return text.Trim();
    }

    /// <summary>
    /// Checks if text starts with a prefix.
    /// </summary>
    public static bool StartsWith(ReadOnlySpan<char> text, ReadOnlySpan<char> prefix)
    {
        return text.StartsWith(prefix);
    }

    /// <summary>
    /// Checks if text ends with a suffix.
    /// </summary>
    public static bool EndsWith(ReadOnlySpan<char> text, ReadOnlySpan<char> suffix)
    {
        return text.EndsWith(suffix);
    }

    /// <summary>
    /// Calculates visual width of text (accounting for Unicode width).
    /// </summary>
    public static int CalculateWidth(ReadOnlySpan<char> text)
    {
        int width = 0;
        foreach (var c in text)
        {
            // Simple width calculation (can be enhanced for full Unicode support)
            width += c >= 0x4E00 && c <= 0x9FFF ? 2 : 1; // CJK ideographs are double-width
        }
        return width;
    }
}

/// <summary>
/// Enumerator for splitting text by lines without allocation.
/// </summary>
public ref struct SpanLineEnumerator
{
    private ReadOnlySpan<char> _remaining;

    public SpanLineEnumerator(ReadOnlySpan<char> text)
    {
        _remaining = text;
        Current = default;
    }

    public ReadOnlySpan<char> Current { get; private set; }

    public bool MoveNext()
    {
        if (_remaining.IsEmpty)
            return false;

        var index = _remaining.IndexOf('\n');
        if (index == -1)
        {
            Current = _remaining;
            _remaining = ReadOnlySpan<char>.Empty;
            return true;
        }

        Current = _remaining.Slice(0, index);
        _remaining = _remaining.Slice(index + 1);
        return true;
    }

    public SpanLineEnumerator GetEnumerator() => this;
}

/// <summary>
/// Text alignment options.
/// </summary>
public enum TextAlignment
{
    Left,
    Center,
    Right
}

/// <summary>
/// Extension methods for ReadOnlySpan text processing.
/// </summary>
public static class SpanTextExtensions
{
    /// <summary>
    /// Splits span by character without allocation.
    /// </summary>
    public static SpanSplitEnumerator Split(this ReadOnlySpan<char> text, char separator)
    {
        return new SpanSplitEnumerator(text, separator);
    }
}

/// <summary>
/// Enumerator for splitting spans by character.
/// </summary>
public ref struct SpanSplitEnumerator
{
    private ReadOnlySpan<char> _remaining;
    private readonly char _separator;

    public SpanSplitEnumerator(ReadOnlySpan<char> text, char separator)
    {
        _remaining = text;
        _separator = separator;
        Current = default;
    }

    public ReadOnlySpan<char> Current { get; private set; }

    public bool MoveNext()
    {
        if (_remaining.IsEmpty)
            return false;

        var index = _remaining.IndexOf(_separator);
        if (index == -1)
        {
            Current = _remaining;
            _remaining = ReadOnlySpan<char>.Empty;
            return true;
        }

        Current = _remaining.Slice(0, index);
        _remaining = _remaining.Slice(index + 1);
        return true;
    }

    public SpanSplitEnumerator GetEnumerator() => this;
}
