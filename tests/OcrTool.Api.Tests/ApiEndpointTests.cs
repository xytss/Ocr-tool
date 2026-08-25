using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace OcrTool.Api.Tests;

public sealed class ApiEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_endpoint_reports_ok()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        using HttpResponseMessage response = await _client.GetAsync("/health", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            "ok",
            await response.Content.ReadAsStringAsync(cancellationToken),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Pdf_endpoint_returns_results_in_page_order()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        using var pdf = PdfOcrServiceTests.CreateTwoPagePdf();
        using var content = new MultipartFormDataContent();
        using var file = new StreamContent(pdf);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(file, "file", "sample.pdf");

        using HttpResponseMessage response = await _client.PostAsync(
            "/api/ocr/pdf",
            content,
            cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PdfOcrResponse? result = await response.Content.ReadFromJsonAsync<PdfOcrResponse>(
            cancellationToken);
        Assert.NotNull(result);
        Assert.Equal([1, 2], result.Pages.Select(page => page.PageNumber));
    }
}
