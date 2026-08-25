using System.Drawing;
using System.Drawing.Text;
using Xunit;

namespace OcrTool.App.Tests;

public sealed class PpOcrV6Tests
{
    [Fact]
    public void Small_model_recognizes_clear_screen_text()
    {
        using var image = new Bitmap(1000, 220);
        using (Graphics graphics = Graphics.FromImage(image))
        using (var font = new Font("Microsoft YaHei UI", 56F, FontStyle.Regular, GraphicsUnit.Pixel))
        {
            graphics.Clear(Color.White);
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.DrawString("轻量 OCR Test 2026", font, Brushes.Black, new PointF(24F, 52F));
        }

        using var engine = new OcrEngine();
        string text = engine.Recognize(image);

        Assert.Contains("OCR", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2026", text, StringComparison.Ordinal);
    }
}
