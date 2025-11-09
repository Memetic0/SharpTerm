namespace SharpTerm.Core.Widgets;

public enum ProgressBarStyle
{
    /// <summary>Solid block characters</summary>
    Blocks,

    /// <summary>Equal signs and dashes</summary>
    Classic,

    /// <summary>Smooth gradient using partial blocks</summary>
    Smooth,

    /// <summary>Dots and circles</summary>
    Dots,
}

/// <summary>
/// A progress bar widget showing completion percentage.
/// </summary>
public class ProgressBar : Widget
{
    private int _value;
    private int _maximum = 100;

    public int Value
    {
        get => _value;
        set
        {
            var newValue = Math.Clamp(value, 0, _maximum);
            if (_value != newValue)
            {
                _value = newValue;
                OnChanged();
            }
        }
    }

    public int Maximum
    {
        get => _maximum;
        set => _maximum = Math.Max(1, value);
    }

    public bool ShowPercentage { get; set; } = true;
    public ProgressBarStyle Style { get; set; } = ProgressBarStyle.Smooth;
    public char FilledChar { get; set; } = '█';
    public char EmptyChar { get; set; } = '░';
    public Color FilledColor { get; set; } = Color.Green;
    public Color EmptyColor { get; set; } = Color.DarkGray;
    public bool UseGradient { get; set; } = false;

    public ProgressBar()
    {
        // Set default characters based on style
        UpdateCharactersForStyle();
    }

    private void UpdateCharactersForStyle()
    {
        switch (Style)
        {
            case ProgressBarStyle.Blocks:
                FilledChar = '█';
                EmptyChar = '░';
                break;
            case ProgressBarStyle.Classic:
                FilledChar = '=';
                EmptyChar = '-';
                break;
            case ProgressBarStyle.Smooth:
                FilledChar = '█';
                EmptyChar = '░';
                break;
            case ProgressBarStyle.Dots:
                FilledChar = '●';
                EmptyChar = '○';
                break;
        }
    }

    public override void Render(ITerminalDriver driver)
    {
        if (!Visible)
            return;

        // Don't render if bounds are invalid
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var percent = (double)Value / Maximum;

        driver.SetCursorPosition(Bounds.X, Bounds.Y);

        if (ShowPercentage && Bounds.Width >= 10)
        {
            // Reserve space for percentage at the end: "XXX%"
            var percentText = $"{(int)(percent * 100), 3}%";
            var barWidth = Bounds.Width - percentText.Length - 1; // -1 for space
            var filled = (int)(barWidth * percent);
            var empty = barWidth - filled;

            // Render filled portion with gradient if enabled
            if (filled > 0)
            {
                if (UseGradient && filled > 2)
                {
                    // Simple gradient: green -> yellow -> red based on progress
                    Color startColor =
                        percent < 0.5 ? Color.Red
                        : percent < 0.8 ? Color.Yellow
                        : Color.Green;
                    driver.Write(new string(FilledChar, filled), startColor, BackgroundColor);
                }
                else
                {
                    driver.Write(new string(FilledChar, filled), FilledColor, BackgroundColor);
                }
            }

            // Render empty portion
            if (empty > 0)
            {
                driver.Write(new string(EmptyChar, empty), EmptyColor, BackgroundColor);
            }

            // Render percentage at the end with a space separator
            driver.Write(" " + percentText, ForegroundColor, BackgroundColor);
        }
        else
        {
            // Simple bar without percentage
            var filled = (int)(Bounds.Width * percent);
            var empty = Bounds.Width - filled;

            if (filled > 0)
            {
                if (UseGradient && filled > 2)
                {
                    Color startColor =
                        percent < 0.5 ? Color.Red
                        : percent < 0.8 ? Color.Yellow
                        : Color.Green;
                    driver.Write(new string(FilledChar, filled), startColor, BackgroundColor);
                }
                else
                {
                    driver.Write(new string(FilledChar, filled), FilledColor, BackgroundColor);
                }
            }
            if (empty > 0)
            {
                driver.Write(new string(EmptyChar, empty), EmptyColor, BackgroundColor);
            }
        }
    }
}
