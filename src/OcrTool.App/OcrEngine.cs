using System.IO;
using SkiaSharp;

namespace OcrTool.App;

internal sealed class OcrEngine : IDisposable
{
    private readonly OcrTool.Engine.OcrEngine _engine = new();

    public string Recognize(System.Drawing.Bitmap image)
    {
        using var stream = new MemoryStream();
        image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Position = 0;

        using SKBitmap bitmap = SKBitmap.Decode(stream);
        return _engine.Recognize(bitmap);
    }

    public void Dispose()
    {
        _engine.Dispose();
    }
}
