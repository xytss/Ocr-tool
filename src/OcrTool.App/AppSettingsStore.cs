using System.IO;
using OcrTool.Core;

namespace OcrTool.App;

internal sealed class AppSettingsStore
{
    private readonly string _path;

    public AppSettingsStore(string path)
    {
        _path = path;
    }

    public AppSettings Load()
    {
        if (!File.Exists(_path))
        {
            return AppSettings.Default;
        }

        return AppSettingsJson.Deserialize(File.ReadAllText(_path));
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, AppSettingsJson.Serialize(settings));
    }
}
