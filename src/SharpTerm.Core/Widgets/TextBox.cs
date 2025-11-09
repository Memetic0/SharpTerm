namespace SharpTerm.Core.Widgets;

/// <summary>
/// A text input widget for user input.
/// </summary>
public class TextBox : Widget
{
    private string _text = string.Empty;
    private int _cursorPosition;

    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            _cursorPosition = _text.Length; // Move cursor to end when setting text
        }
    }

    public int MaxLength { get; set; } = 100;
    public bool IsFocused { get; set; }
    public char PasswordChar { get; set; } = '\0'; // Use non-zero for password field
    public string Placeholder { get; set; } = string.Empty;

    public event EventHandler<string>? TextChanged;
    public event EventHandler<string>? Submit;

    public override void Render(ITerminalDriver driver)
    {
        if (!Visible)
            return;

        // Don't render if bounds are invalid
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var displayText = GetDisplayText();

        // Enhanced color scheme
        Color bg,
            fg,
            cursorBg,
            cursorFg;
        if (IsFocused)
        {
            bg = Color.Blue;
            fg = Color.White;
            cursorBg = Color.White;
            cursorFg = Color.Black;
        }
        else
        {
            bg = BackgroundColor;
            fg = ForegroundColor;
            cursorBg = Color.DarkGray;
            cursorFg = Color.White;
        }

        // Dim color for placeholder
        var placeholderColor = IsFocused ? Color.DarkGray : Color.DarkGray;
        var isPlaceholder = _text.Length == 0 && !string.IsNullOrEmpty(Placeholder) && !IsFocused;

        driver.SetCursorPosition(Bounds.X, Bounds.Y);

        // Render the text or placeholder
        var output = displayText.PadRight(Bounds.Width);
        if (output.Length > Bounds.Width)
        {
            output = output.Substring(0, Bounds.Width);
        }

        // Use special color for placeholder text
        driver.Write(output, isPlaceholder ? placeholderColor : fg, bg);

        // Show cursor position if focused
        if (IsFocused && _cursorPosition < Bounds.Width)
        {
            driver.SetCursorPosition(Bounds.X + _cursorPosition, Bounds.Y);
            if (_cursorPosition < displayText.Length)
            {
                driver.Write(displayText[_cursorPosition].ToString(), cursorFg, cursorBg);
            }
            else
            {
                driver.Write("_", cursorFg, cursorBg);
            }
        }
    }

    private string GetDisplayText()
    {
        if (_text.Length == 0 && !string.IsNullOrEmpty(Placeholder) && !IsFocused)
        {
            return Placeholder;
        }

        if (PasswordChar != '\0' && _text.Length > 0)
        {
            return new string(PasswordChar, _text.Length);
        }

        return _text;
    }

    public override bool HandleKey(ConsoleKeyInfo key)
    {
        if (!IsFocused)
            return false;

        switch (key.Key)
        {
            case ConsoleKey.Backspace:
                if (_cursorPosition > 0)
                {
                    _text = _text.Remove(_cursorPosition - 1, 1);
                    _cursorPosition--;
                    TextChanged?.Invoke(this, _text);
                }
                return true;

            case ConsoleKey.Delete:
                if (_cursorPosition < _text.Length)
                {
                    _text = _text.Remove(_cursorPosition, 1);
                    TextChanged?.Invoke(this, _text);
                }
                return true;

            case ConsoleKey.LeftArrow:
                _cursorPosition = Math.Max(0, _cursorPosition - 1);
                return true;

            case ConsoleKey.RightArrow:
                _cursorPosition = Math.Min(_text.Length, _cursorPosition + 1);
                return true;

            case ConsoleKey.Home:
                _cursorPosition = 0;
                return true;

            case ConsoleKey.End:
                _cursorPosition = _text.Length;
                return true;

            case ConsoleKey.Enter:
                Submit?.Invoke(this, _text);
                return true;

            default:
                // Add printable characters
                if (!char.IsControl(key.KeyChar) && _text.Length < MaxLength)
                {
                    _text = _text.Insert(_cursorPosition, key.KeyChar.ToString());
                    _cursorPosition++;
                    TextChanged?.Invoke(this, _text);
                    return true;
                }
                break;
        }

        return false;
    }
}
