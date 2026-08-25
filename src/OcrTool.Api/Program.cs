using OcrTool.Api;
using OcrTool.Engine;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<OcrEngine>();
builder.Services.AddSingleton<PdfOcrService>();

WebApplication app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapPost(
        "/api/ocr/pdf",
        async (IFormFile file, PdfOcrService service, CancellationToken cancellationToken) =>
        {
            await using Stream stream = file.OpenReadStream();
            PdfOcrResponse response = await service.RecognizeAsync(stream, cancellationToken);
            return Results.Ok(response);
        })
    .DisableAntiforgery();
app.Run();

public partial class Program;
