namespace SharpTerm.Core.Widgets;

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
    public char FilledChar { get; set; } = '█';
    public char EmptyChar { get; set; } = '░';
    public Color FilledColor { get; set; } = Color.Green;
    public Color EmptyColor { get; set; } = Color.DarkGray;
    
    public ProgressBar()
    {
        // Use ASCII-compatible characters if Unicode blocks don't render properly
        // Users can override these with the Unicode block characters if their terminal supports it
        FilledChar = '=';
        EmptyChar = '-';
    }
    
    public override void Render(ITerminalDriver driver)
    {
        if (!Visible) return;
        
        var percent = (double)Value / Maximum;
        var filled = (int)(Bounds.Width * percent);
        var empty = Bounds.Width - filled;
        
        driver.SetCursorPosition(Bounds.X, Bounds.Y);
        
        // Render filled portion
        if (filled > 0)
        {
            driver.Write(new string(FilledChar, filled), FilledColor, BackgroundColor);
        }
        
        // Render empty portion
        if (empty > 0)
        {
            driver.Write(new string(EmptyChar, empty), EmptyColor, BackgroundColor);
        }
        
        // Overlay percentage text in the center if enabled
        if (ShowPercentage && Bounds.Width >= 6)
        {
            var percentText = $"{(int)(percent * 100),3}%";
            var textX = Bounds.X + (Bounds.Width - percentText.Length) / 2;
            driver.SetCursorPosition(textX, Bounds.Y);
            
            // Use inverted colors for better visibility
            var textColor = percent > 0.5 ? Color.White : ForegroundColor;
            var textBg = percent > 0.5 ? FilledColor : BackgroundColor;
            driver.Write(percentText, textColor, textBg);
        }
    }
}
