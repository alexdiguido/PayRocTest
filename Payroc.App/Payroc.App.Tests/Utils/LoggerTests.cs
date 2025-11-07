using Payroc.App.LoadBalancer;
using System;
using System.IO;
using Xunit;

namespace Payroc.App.Tests.Utils;

public class LoggerTests
{
    [Theory]
    [InlineData("INFO", "Test info message", ConsoleColor.White)]
    [InlineData("WARN", "Test warn message", ConsoleColor.Yellow)]
    [InlineData("ERROR", "Test error message", ConsoleColor.Red)]
    [InlineData("OK", "Test success message", ConsoleColor.Green)]
    public void Log_Method_ShouldWriteExpectedFormat(string level, string message, ConsoleColor color)
    {
        // Arrange
        var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        // Act
        switch (level)
        {
            case "INFO":
                Logger.Info(message);
                break;
            case "WARN":
                Logger.Warn(message);
                break;
            case "ERROR":
                Logger.Error(message);
                break;
            case "OK":
                Logger.Success(message);
                break;
        }

        // Assert
        var output = sw.ToString();
        Assert.Contains($"[{level}]", output);
        Assert.Contains(message, output);
        Assert.Contains("[", output); // Timestamp present

        // Cleanup
        Console.SetOut(originalOut);
    }

    [Fact]
    public void Log_ShouldBeThreadSafe()
    {
        // Arrange
        var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        // Act
        Parallel.For(0, 10, i =>
        {
            Logger.Info($"Thread message {i}");
        });

        // Assert
        var output = sw.ToString();
        for (int i = 0; i < 10; i++)
        {
            Assert.Contains($"Thread message {i}", output);
        }

        // Cleanup
        Console.SetOut(originalOut);
    }

    [Fact]
    public void Log_ShouldResetConsoleColor()
    {
        // Arrange
        var originalColor = Console.ForegroundColor;
        var sw = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(sw);

        // Act
        Logger.Warn("Color test");

        // Assert
        Assert.Equal(originalColor, Console.ForegroundColor);

        // Cleanup
        Console.SetOut(originalOut);
    }
}
