namespace SharpTerm.Core.Theming;

/// <summary>
/// Defines a color scheme and styling for widgets.
/// </summary>
public class Theme
{
    // General colors
    public Color PrimaryColor { get; set; } = Color.Blue;
    public Color SecondaryColor { get; set; } = Color.Cyan;
    public Color AccentColor { get; set; } = Color.Yellow;
    public Color BackgroundColor { get; set; } = Color.Black;
    public Color ForegroundColor { get; set; } = Color.White;
    public Color ErrorColor { get; set; } = Color.Red;
    public Color SuccessColor { get; set; } = Color.Green;
    public Color WarningColor { get; set; } = Color.Yellow;

    // Widget-specific colors
    public Color ButtonFocusedBackground { get; set; } = Color.Blue;
    public Color ButtonFocusedForeground { get; set; } = Color.White;
    public Color ButtonNormalBackground { get; set; } = Color.Transparent;
    public Color ButtonNormalForeground { get; set; } = Color.White;

    public Color TextBoxFocusedBackground { get; set; } = Color.Blue;
    public Color TextBoxFocusedForeground { get; set; } = Color.White;
    public Color TextBoxNormalBackground { get; set; } = Color.Transparent;
    public Color TextBoxNormalForeground { get; set; } = Color.White;

    public Color ListSelectedBackground { get; set; } = Color.Blue;
    public Color ListSelectedForeground { get; set; } = Color.White;
    public Color ListAlternateRowBackground { get; set; } = Color.DarkGray;

    public Color BorderColor { get; set; } = Color.Cyan;
    public Color TitleColor { get; set; } = Color.Yellow;

    public Color ProgressBarFilledColor { get; set; } = Color.Green;
    public Color ProgressBarEmptyColor { get; set; } = Color.DarkGray;

    // Border styles
    public Widgets.BorderStyle DefaultBorderStyle { get; set; } = Widgets.BorderStyle.Single;

    /// <summary>
    /// Default light theme.
    /// </summary>
    public static Theme Light => new()
    {
        PrimaryColor = new Color(0, 122, 204),
        SecondaryColor = new Color(100, 100, 100),
        AccentColor = new Color(255, 165, 0),
        BackgroundColor = Color.White,
        ForegroundColor = Color.Black,
        ErrorColor = new Color(220, 50, 47),
        SuccessColor = new Color(133, 153, 0),
        WarningColor = new Color(181, 137, 0),
        ButtonFocusedBackground = new Color(0, 122, 204),
        ButtonFocusedForeground = Color.White,
        ButtonNormalBackground = new Color(230, 230, 230),
        ButtonNormalForeground = Color.Black,
        TextBoxFocusedBackground = Color.White,
        TextBoxFocusedForeground = Color.Black,
        TextBoxNormalBackground = new Color(245, 245, 245),
        TextBoxNormalForeground = Color.Black,
        ListSelectedBackground = new Color(0, 122, 204),
        ListSelectedForeground = Color.White,
        ListAlternateRowBackground = new Color(245, 245, 245),
        BorderColor = new Color(100, 100, 100),
        TitleColor = new Color(0, 122, 204),
        ProgressBarFilledColor = new Color(133, 153, 0),
        ProgressBarEmptyColor = new Color(230, 230, 230)
    };

    /// <summary>
    /// Default dark theme.
    /// </summary>
    public static Theme Dark => new()
    {
        PrimaryColor = new Color(0, 122, 204),
        SecondaryColor = new Color(150, 150, 150),
        AccentColor = new Color(255, 165, 0),
        BackgroundColor = new Color(30, 30, 30),
        ForegroundColor = new Color(220, 220, 220),
        ErrorColor = new Color(220, 50, 47),
        SuccessColor = new Color(133, 153, 0),
        WarningColor = new Color(181, 137, 0),
        ButtonFocusedBackground = new Color(0, 122, 204),
        ButtonFocusedForeground = Color.White,
        ButtonNormalBackground = new Color(60, 60, 60),
        ButtonNormalForeground = new Color(220, 220, 220),
        TextBoxFocusedBackground = new Color(0, 122, 204),
        TextBoxFocusedForeground = Color.White,
        TextBoxNormalBackground = new Color(50, 50, 50),
        TextBoxNormalForeground = new Color(220, 220, 220),
        ListSelectedBackground = new Color(0, 122, 204),
        ListSelectedForeground = Color.White,
        ListAlternateRowBackground = new Color(40, 40, 40),
        BorderColor = new Color(100, 100, 100),
        TitleColor = new Color(0, 150, 255),
        ProgressBarFilledColor = new Color(133, 153, 0),
        ProgressBarEmptyColor = new Color(60, 60, 60)
    };

    /// <summary>
    /// High contrast theme for accessibility.
    /// </summary>
    public static Theme HighContrast => new()
    {
        PrimaryColor = Color.Yellow,
        SecondaryColor = Color.White,
        AccentColor = Color.Cyan,
        BackgroundColor = Color.Black,
        ForegroundColor = Color.White,
        ErrorColor = Color.Red,
        SuccessColor = Color.Green,
        WarningColor = Color.Yellow,
        ButtonFocusedBackground = Color.Yellow,
        ButtonFocusedForeground = Color.Black,
        ButtonNormalBackground = Color.Black,
        ButtonNormalForeground = Color.White,
        TextBoxFocusedBackground = Color.Yellow,
        TextBoxFocusedForeground = Color.Black,
        TextBoxNormalBackground = Color.Black,
        TextBoxNormalForeground = Color.White,
        ListSelectedBackground = Color.Yellow,
        ListSelectedForeground = Color.Black,
        ListAlternateRowBackground = Color.Black,
        BorderColor = Color.White,
        TitleColor = Color.Yellow,
        ProgressBarFilledColor = Color.Green,
        ProgressBarEmptyColor = Color.White,
        DefaultBorderStyle = Widgets.BorderStyle.Double
    };

    /// <summary>
    /// Applies this theme to a widget.
    /// </summary>
    public virtual void ApplyTo(Widget widget)
    {
        widget.ForegroundColor = ForegroundColor;
        widget.BackgroundColor = BackgroundColor;

        // Apply widget-specific styling
        switch (widget)
        {
            case Widgets.Button button:
                ApplyToButton(button);
                break;
            case Widgets.TextBox textBox:
                ApplyToTextBox(textBox);
                break;
            case Widgets.List list:
                ApplyToList(list);
                break;
            case Widgets.Border border:
                ApplyToBorder(border);
                break;
            case Widgets.ProgressBar progressBar:
                ApplyToProgressBar(progressBar);
                break;
        }
    }

    protected virtual void ApplyToButton(Widgets.Button button)
    {
        button.ForegroundColor = ButtonNormalForeground;
        button.BackgroundColor = ButtonNormalBackground;
    }

    protected virtual void ApplyToTextBox(Widgets.TextBox textBox)
    {
        textBox.ForegroundColor = TextBoxNormalForeground;
        textBox.BackgroundColor = TextBoxNormalBackground;
    }

    protected virtual void ApplyToList(Widgets.List list)
    {
        list.ForegroundColor = ForegroundColor;
        list.BackgroundColor = BackgroundColor;
        list.SelectedColor = ListSelectedBackground;
        list.AlternateRowColor = ListAlternateRowBackground;
    }

    protected virtual void ApplyToBorder(Widgets.Border border)
    {
        border.ForegroundColor = BorderColor;
        border.BackgroundColor = BackgroundColor;
        border.Style = DefaultBorderStyle;
    }

    protected virtual void ApplyToProgressBar(Widgets.ProgressBar progressBar)
    {
        progressBar.FilledColor = ProgressBarFilledColor;
        progressBar.EmptyColor = ProgressBarEmptyColor;
    }
}
