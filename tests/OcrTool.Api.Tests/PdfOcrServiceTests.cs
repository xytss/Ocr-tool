using System.Globalization;
using System.Text;
using OcrTool.Engine;
using Xunit;

namespace OcrTool.Api.Tests;

public sealed class PdfOcrServiceTests
{
    [Fact]
    public async Task Service_returns_ocr_text_for_each_pdf_page()
    {
        using var pdf = CreateTwoPagePdf();
        using var engine = new OcrEngine();
        var service = new PdfOcrService(engine);

        PdfOcrResponse result = await service.RecognizeAsync(pdf, CancellationToken.None);

        Assert.Equal(2, result.PageCount);
        Assert.Collection(
            result.Pages,
            page =>
            {
                Assert.Equal(1, page.PageNumber);
                Assert.Contains("2026", page.Text, StringComparison.Ordinal);
            },
            page =>
            {
                Assert.Equal(2, page.PageNumber);
                Assert.Contains("TWO", page.Text, StringComparison.OrdinalIgnoreCase);
            });
    }

    internal static MemoryStream CreateTwoPagePdf()
    {
        const string firstPage = "BT /F1 54 Tf 72 600 Td (PAGE ONE OCR 2026) Tj ET";
        const string secondPage = "BT /F1 54 Tf 72 600 Td (PAGE TWO OCR 2027) Tj ET";
        string[] objects =
        [
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R 5 0 R] /Count 2 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 7 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(firstPage)} >>\nstream\n{firstPage}\nendstream",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 7 0 R >> >> /Contents 6 0 R >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(secondPage)} >>\nstream\n{secondPage}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        ];

        var stream = new MemoryStream();
        WriteAscii(stream, "%PDF-1.4\n");
        var offsets = new List<long>(objects.Length);

        for (int index = 0; index < objects.Length; index++)
        {
            offsets.Add(stream.Position);
            WriteAscii(stream, $"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
        }

        long xrefOffset = stream.Position;
        WriteAscii(stream, $"xref\n0 {objects.Length + 1}\n");
        WriteAscii(stream, "0000000000 65535 f \n");
        foreach (long offset in offsets)
        {
            WriteAscii(stream, $"{offset.ToString("D10", CultureInfo.InvariantCulture)} 00000 n \n");
        }

        WriteAscii(
            stream,
            $"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");
        stream.Position = 0;
        return stream;
    }

    private static void WriteAscii(Stream stream, string value)
    {
        stream.Write(Encoding.ASCII.GetBytes(value));
    }
}
