using System.Drawing;
using Xunit;

namespace OcrTool.App.Tests;

public sealed class BitmapSourceFactoryTests
{
    [Fact]
    public void Converted_screenshot_is_detached_from_source_bitmap()
    {
        using var bitmap = new Bitmap(320, 180);
        Type? factoryType = typeof(App).Assembly.GetType("OcrTool.App.BitmapSourceFactory");
        object? imageSource = factoryType?.GetMethod("Create")?.Invoke(null, [bitmap]);

        Assert.NotNull(imageSource);
        Assert.Equal(320, imageSource.GetType().GetProperty("PixelWidth")!.GetValue(imageSource));
        Assert.Equal(180, imageSource.GetType().GetProperty("PixelHeight")!.GetValue(imageSource));
        Assert.True((bool)imageSource.GetType().GetProperty("IsFrozen")!.GetValue(imageSource)!);
    }
}
