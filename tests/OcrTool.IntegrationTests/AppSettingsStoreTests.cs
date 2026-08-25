using System.Reflection;
using OcrTool.Core;
using Xunit;

namespace OcrTool.App.Tests;

public sealed class AppSettingsStoreTests
{
    [Fact]
    public void Portable_mode_stores_settings_beside_the_executable()
    {
        Type? pathType = typeof(App).Assembly.GetType("OcrTool.App.AppStoragePath");
        MethodInfo? resolve = pathType?.GetMethod("Resolve", BindingFlags.Static | BindingFlags.Public);

        Assert.NotNull(resolve);
        Assert.Equal(
            "F:\\Portable OCR\\settings.json",
            resolve.Invoke(null, ["F:\\Portable OCR", "C:\\Users\\User\\AppData\\Local", true]));
    }

    [Fact]
    public void Installed_mode_stores_settings_in_local_app_data()
    {
        Type? pathType = typeof(App).Assembly.GetType("OcrTool.App.AppStoragePath");
        MethodInfo? resolve = pathType?.GetMethod("Resolve", BindingFlags.Static | BindingFlags.Public);

        Assert.NotNull(resolve);
        Assert.Equal(
            "C:\\Users\\User\\AppData\\Local\\OcrTool\\settings.json",
            resolve.Invoke(null, ["C:\\Program Files\\OCR Tool", "C:\\Users\\User\\AppData\\Local", false]));
    }

    [Fact]
    public void Store_saves_and_loads_settings_from_its_file()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "settings-store-test.json");
        Type? storeType = typeof(App).Assembly.GetType("OcrTool.App.AppSettingsStore");
        object? store = storeType is null ? null : Activator.CreateInstance(storeType, path);
        var expected = new AppSettings
        {
            ShowResultWindow = false,
            AutoCopy = true,
            Hotkey = new HotkeyGesture(HotkeyModifiers.Alt, 0x51, "Q")
        };

        storeType?.GetMethod("Save")?.Invoke(store, [expected]);
        AppSettings? actual = storeType?.GetMethod("Load")?.Invoke(store, null) as AppSettings;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Store_creates_the_settings_directory_when_needed()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "installed-settings-test",
            "settings.json");
        Type storeType = typeof(App).Assembly.GetType("OcrTool.App.AppSettingsStore")!;
        object store = Activator.CreateInstance(storeType, path)!;

        storeType.GetMethod("Save")!.Invoke(store, [AppSettings.Default]);

        Assert.True(File.Exists(path));
    }
}
