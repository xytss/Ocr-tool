using OcrTool.Engine;
using SkiaSharp;
using Xunit;

namespace OcrTool.Api.Tests;

public sealed class CrossPlatformOcrTests
{
    [Fact]
    public void Engine_recognizes_clear_skia_text()
    {
        using var image = new SKBitmap(1000, 220);
        using (var canvas = new SKCanvas(image))
        using (var font = new SKFont(SKTypeface.FromFamilyName("Arial"), 58F))
        using (var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true })
        {
            canvas.Clear(SKColors.White);
            canvas.DrawText("OCR API 2026", 32F, 130F, SKTextAlign.Left, font, paint);
        }

        using var engine = new OcrEngine();
        string text = engine.Recognize(image);

        Assert.Contains("OCR", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2026", text, StringComparison.Ordinal);
    }
}
