namespace SharpTerm.Core.Performance;

/// <summary>
/// Helper methods for working with Span<T> in rendering operations.
/// </summary>
public static class SpanHelpers
{
    /// <summary>
    /// Creates a padded string using span to avoid allocations.
    /// </summary>
    public static string PadRight(ReadOnlySpan<char> text, int totalWidth, char paddingChar = ' ')
    {
        if (text.Length >= totalWidth)
            return text.ToString();

        return string.Create(totalWidth, (text: text.ToString(), paddingChar), (span, state) =>
        {
            state.text.AsSpan().CopyTo(span);
            span[state.text.Length..].Fill(state.paddingChar);
        });
    }

    /// <summary>
    /// Creates a padded string on the left using span.
    /// </summary>
    public static string PadLeft(ReadOnlySpan<char> text, int totalWidth, char paddingChar = ' ')
    {
        if (text.Length >= totalWidth)
            return text.ToString();

        return string.Create(totalWidth, (text: text.ToString(), paddingChar), (span, state) =>
        {
            int paddingLength = span.Length - state.text.Length;
            span[..paddingLength].Fill(state.paddingChar);
            state.text.AsSpan().CopyTo(span[paddingLength..]);
        });
    }

    /// <summary>
    /// Centers text within a given width using span.
    /// </summary>
    public static string Center(ReadOnlySpan<char> text, int totalWidth, char paddingChar = ' ')
    {
        if (text.Length >= totalWidth)
            return text.ToString();

        return string.Create(totalWidth, (text: text.ToString(), paddingChar), (span, state) =>
        {
            int leftPadding = (span.Length - state.text.Length) / 2;
            int rightPadding = span.Length - state.text.Length - leftPadding;

            span[..leftPadding].Fill(state.paddingChar);
            state.text.AsSpan().CopyTo(span[leftPadding..]);
            span[(leftPadding + state.text.Length)..].Fill(state.paddingChar);
        });
    }

    /// <summary>
    /// Truncates text to fit within a specified width using span.
    /// </summary>
    public static string Truncate(ReadOnlySpan<char> text, int maxWidth)
    {
        if (text.Length <= maxWidth)
            return text.ToString();

        return text[..maxWidth].ToString();
    }

    /// <summary>
    /// Creates a string of repeated characters efficiently.
    /// </summary>
    public static string Repeat(char character, int count)
    {
        if (count <= 0)
            return string.Empty;

        return string.Create(count, character, (span, ch) => span.Fill(ch));
    }

    /// <summary>
    /// Splits text by newline and processes each line with minimal allocations.
    /// </summary>
    public static void ProcessLines(ReadOnlySpan<char> text, Action<ReadOnlySpan<char>, int> lineAction)
    {
        int lineIndex = 0;
        int start = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                lineAction(text[start..i], lineIndex++);
                start = i + 1;
            }
        }

        // Process last line if any
        if (start < text.Length)
        {
            lineAction(text[start..], lineIndex);
        }
    }

    /// <summary>
    /// Gets line count efficiently without allocating strings.
    /// </summary>
    public static int GetLineCount(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
            return 0;

        int count = 1;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
                count++;
        }

        return count;
    }
}
