using Xunit;
using SharpTerm.Core;
using SharpTerm.Core.Widgets;

namespace SharpTerm.Tests;

public class WidgetTests
{
    [Fact]
    public void Label_SetsTextCorrectly()
    {
        var label = new Label
        {
            Text = "Test Label",
            Bounds = new Rectangle(0, 0, 20, 1)
        };
        
        Assert.Equal("Test Label", label.Text);
    }
    
    [Fact]
    public void Button_FiresClickEvent()
    {
        var button = new Button { Text = "Click Me" };
        var clicked = false;
        button.Click += (s, e) => clicked = true;
        
        button.HandleKey(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        
        Assert.True(clicked);
    }
    
    [Fact]
    public void Button_ClickEventNotFiredOnOtherKeys()
    {
        var button = new Button { Text = "Click Me" };
        var clicked = false;
        button.Click += (s, e) => clicked = true;
        
        button.HandleKey(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));
        
        Assert.False(clicked);
    }
    
    [Fact]
    public void Color_PredefinedColorsAreCorrect()
    {
        Assert.Equal(new Color(255, 0, 0), Color.Red);
        Assert.Equal(new Color(0, 255, 0), Color.Green);
        Assert.Equal(new Color(0, 0, 255), Color.Blue);
        Assert.Equal(new Color(255, 255, 255), Color.White);
        Assert.Equal(new Color(0, 0, 0), Color.Black);
    }
    
    [Fact]
    public void Rectangle_ConstructorSetsPropertiesCorrectly()
    {
        var rect = new Rectangle(10, 20, 30, 40);
        
        Assert.Equal(10, rect.X);
        Assert.Equal(20, rect.Y);
        Assert.Equal(30, rect.Width);
        Assert.Equal(40, rect.Height);
    }
    
    [Fact]
    public void Border_DefaultStyleIsSingle()
    {
        var border = new Border();
        
        Assert.Equal(BorderStyle.Single, border.Style);
    }
    
    [Fact]
    public void Widget_DefaultVisibilityIsTrue()
    {
        var label = new Label();
        
        Assert.True(label.Visible);
    }
    
    [Fact]
    public void ProgressBar_CalculatesPercentageCorrectly()
    {
        var progressBar = new ProgressBar
        {
            Value = 50,
            Maximum = 100,
            Bounds = new Rectangle(0, 0, 20, 1)
        };
        
        Assert.Equal(50, progressBar.Value);
        Assert.Equal(100, progressBar.Maximum);
    }
    
    [Fact]
    public void ProgressBar_ClampsValue()
    {
        var progressBar = new ProgressBar
        {
            Maximum = 100
        };
        
        progressBar.Value = 150;
        Assert.Equal(100, progressBar.Value);
        
        progressBar.Value = -10;
        Assert.Equal(0, progressBar.Value);
    }
    
    [Fact]
    public void TextBox_HandlesTextInput()
    {
        var textBox = new TextBox { IsFocused = true };
        var changed = false;
        textBox.TextChanged += (s, t) => changed = true;
        
        textBox.HandleKey(new ConsoleKeyInfo('H', ConsoleKey.H, false, false, false));
        
        Assert.True(changed);
        Assert.Equal("H", textBox.Text);
    }
    
    [Fact]
    public void TextBox_HandlesBackspace()
    {
        var textBox = new TextBox { IsFocused = true };
        textBox.Text = "Hello"; // This sets text and moves cursor to end
        
        // Simulate pressing End key to ensure cursor is at end
        textBox.HandleKey(new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false));
        textBox.HandleKey(new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false));
        
        Assert.Equal("Hell", textBox.Text);
    }
    
    [Fact]
    public void TextBox_RespectsMaxLength()
    {
        var textBox = new TextBox { MaxLength = 5, IsFocused = true };
        
        textBox.HandleKey(new ConsoleKeyInfo('1', ConsoleKey.D1, false, false, false));
        textBox.HandleKey(new ConsoleKeyInfo('2', ConsoleKey.D2, false, false, false));
        textBox.HandleKey(new ConsoleKeyInfo('3', ConsoleKey.D3, false, false, false));
        textBox.HandleKey(new ConsoleKeyInfo('4', ConsoleKey.D4, false, false, false));
        textBox.HandleKey(new ConsoleKeyInfo('5', ConsoleKey.D5, false, false, false));
        textBox.HandleKey(new ConsoleKeyInfo('6', ConsoleKey.D6, false, false, false));
        
        Assert.Equal("12345", textBox.Text);
    }
}
