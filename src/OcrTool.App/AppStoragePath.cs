using System.IO;

namespace OcrTool.App;

internal static class AppStoragePath
{
    public static string Resolve(
        string baseDirectory,
        string localApplicationDataDirectory,
        bool portable)
    {
        string directory = portable
            ? baseDirectory
            : Path.Combine(localApplicationDataDirectory, "OcrTool");

        return Path.Combine(directory, "settings.json");
    }
}
