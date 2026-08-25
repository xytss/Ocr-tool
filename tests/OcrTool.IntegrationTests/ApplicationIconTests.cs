using Xunit;

namespace OcrTool.App.Tests;

public sealed class ApplicationIconTests
{
    [Fact]
    public void Application_project_embeds_custom_icon()
    {
        string repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        string projectPath = Path.Combine(repositoryRoot, "src", "OcrTool.App", "OcrTool.App.csproj");
        string iconPath = Path.Combine(repositoryRoot, "src", "OcrTool.App", "Assets", "OcrTool.ico");
        string project = File.ReadAllText(projectPath);

        Assert.Contains("<ApplicationIcon>Assets\\OcrTool.ico</ApplicationIcon>", project);
        Assert.True(File.Exists(iconPath));
    }
}
