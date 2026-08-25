using OcrTool.Engine;
using PDFtoImage;
using SkiaSharp;

namespace OcrTool.Api;

public sealed record PdfPageOcrResult(int PageNumber, string Text);

public sealed record PdfOcrResponse(int PageCount, IReadOnlyList<PdfPageOcrResult> Pages);

public sealed class PdfOcrService(OcrEngine engine)
{
    public Task<PdfOcrResponse> RecognizeAsync(
        Stream pdfStream,
        CancellationToken cancellationToken)
    {
        return RecognizePagesAsync(pdfStream, cancellationToken);
    }

    private async Task<PdfOcrResponse> RecognizePagesAsync(
        Stream pdfStream,
        CancellationToken cancellationToken)
    {
        var pages = new List<PdfPageOcrResult>();
        var renderOptions = new RenderOptions(Dpi: 180);

        await foreach (SKBitmap image in Conversion.ToImagesAsync(
            pdfStream,
            leaveOpen: true,
            options: renderOptions,
            cancellationToken: cancellationToken))
        {
            using (image)
            {
                string text = engine.Recognize(image);
                pages.Add(new PdfPageOcrResult(pages.Count + 1, text));
            }
        }

        return new PdfOcrResponse(pages.Count, pages);
    }
}
