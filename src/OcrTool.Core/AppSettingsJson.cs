using System.Text.Json;

namespace OcrTool.Core;

public static class AppSettingsJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static string Serialize(AppSettings settings)
    {
        return JsonSerializer.Serialize(settings, Options);
    }

    public static AppSettings Deserialize(string json)
    {
        return JsonSerializer.Deserialize<AppSettings>(json, Options)!;
    }
}
