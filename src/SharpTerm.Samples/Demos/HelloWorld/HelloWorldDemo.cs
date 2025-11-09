using SharpTerm.Core;

namespace SharpTerm.Samples.Demos.HelloWorld;

public static class HelloWorldDemo
{
    public static void Run()
    {
        using var driver = new AnsiTerminalDriver();
        driver.Clear();
        driver.SetCursorPosition(0, 0);
        driver.Write("Hello, SharpTerm!", Color.Cyan, Color.Black);
        driver.SetCursorPosition(0, 2);
        driver.Write("Press any key to return to menu...", Color.Yellow, Color.Black);
        driver.Flush();
        Console.ReadKey(true);
    }
}
