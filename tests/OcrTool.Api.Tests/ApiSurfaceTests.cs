using System.Reflection;
using SkiaSharp;
using Xunit;

namespace OcrTool.Api.Tests;

public sealed class ApiSurfaceTests
{
    [Fact]
    public void Cross_platform_ocr_engine_is_available()
    {
        Assembly assembly = Assembly.Load("OcrTool.Engine");

        Assert.NotNull(assembly.GetType("OcrTool.Engine.OcrEngine"));
    }

    [Fact]
    public void Pdf_ocr_service_is_available()
    {
        Assembly assembly = Assembly.Load("OcrTool.Api");

        Assert.NotNull(assembly.GetType("OcrTool.Api.PdfOcrService"));
    }

    [Fact]
    public void Cross_platform_engine_accepts_skia_bitmap()
    {
        Assembly assembly = Assembly.Load("OcrTool.Engine");
        Type engineType = assembly.GetType("OcrTool.Engine.OcrEngine")!;

        Assert.NotNull(engineType.GetMethod("Recognize", [typeof(SKBitmap)]));
    }

    [Fact]
    public void Cross_platform_engine_owns_disposable_native_sessions()
    {
        Assembly assembly = Assembly.Load("OcrTool.Engine");
        Type engineType = assembly.GetType("OcrTool.Engine.OcrEngine")!;

        Assert.True(typeof(IDisposable).IsAssignableFrom(engineType));
    }

    [Fact]
    public void Pdf_service_exposes_async_stream_recognition()
    {
        Assembly assembly = Assembly.Load("OcrTool.Api");
        Type serviceType = assembly.GetType("OcrTool.Api.PdfOcrService")!;

        Assert.NotNull(serviceType.GetMethod(
            "RecognizeAsync",
            [typeof(Stream), typeof(CancellationToken)]));
    }

    [Fact]
    public void Http_host_entry_point_is_available()
    {
        Assembly assembly = Assembly.Load("OcrTool.Api");

        Assert.NotNull(assembly.GetType("Program"));
    }
}
