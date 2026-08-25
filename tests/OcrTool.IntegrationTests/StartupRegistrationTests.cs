using System.Reflection;
using OcrTool.App;
using Xunit;

namespace OcrTool.IntegrationTests;

public sealed class StartupRegistrationTests
{
    [Fact]
    public void Startup_command_quotes_the_executable_path()
    {
        Type? registrationType = typeof(OcrTool.App.App).Assembly.GetType("OcrTool.App.StartupRegistration");
        MethodInfo? method = registrationType?.GetMethod(
            "CommandForExecutable",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(method);
        Assert.Equal(
            "\"F:\\Portable OCR\\OcrTool.App.exe\"",
            method.Invoke(null, ["F:\\Portable OCR\\OcrTool.App.exe"]));
    }
}
