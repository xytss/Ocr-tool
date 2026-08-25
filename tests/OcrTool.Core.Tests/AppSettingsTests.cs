using OcrTool.Core;
using System.Reflection;
using Xunit;

namespace OcrTool.Core.Tests;

public sealed class AppSettingsTests
{
    [Fact]
    public void Default_settings_enable_result_window_and_automatic_copy()
    {
        Assembly assembly = Assembly.Load("OcrTool.Core");
        Type? settingsType = assembly.GetType("OcrTool.Core.AppSettings");
        object? settings = settingsType?
            .GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?
            .GetValue(null);

        Assert.NotNull(settings);
        Assert.True((bool)settingsType!.GetProperty("ShowResultWindow")!.GetValue(settings)!);
        Assert.True((bool)settingsType.GetProperty("AutoCopy")!.GetValue(settings)!);
    }

    [Fact]
    public void Default_settings_do_not_start_with_windows()
    {
        AppSettings settings = AppSettings.Default;

        Assert.False(settings.StartWithWindows);
    }

    [Fact]
    public void Default_settings_use_f2()
    {
        Assembly assembly = Assembly.Load("OcrTool.Core");
        Type settingsType = assembly.GetType("OcrTool.Core.AppSettings")!;
        object settings = settingsType
            .GetProperty("Default", BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null)!;
        object? hotkey = settingsType.GetProperty("Hotkey")?.GetValue(settings);
        string? displayText = hotkey?
            .GetType()
            .GetProperty("DisplayText")?
            .GetValue(hotkey) as string;

        Assert.Equal("F2", displayText);
    }

    [Fact]
    public void Function_keys_can_be_used_without_modifiers()
    {
        var hotkey = new HotkeyGesture(0, 0x71, "F2");
        bool? allowed = hotkey
            .GetType()
            .GetProperty("CanUseWithoutModifiers")?
            .GetValue(hotkey) as bool?;

        Assert.True(allowed);
    }

    [Fact]
    public void Settings_json_round_trip_preserves_user_choices()
    {
        var expected = new AppSettings
        {
            ShowResultWindow = false,
            AutoCopy = false,
            StartWithWindows = true,
            Hotkey = new HotkeyGesture(HotkeyModifiers.Alt, 0x51, "Q")
        };
        Type? jsonType = typeof(AppSettings).Assembly.GetType("OcrTool.Core.AppSettingsJson");
        MethodInfo? serialize = jsonType?.GetMethod("Serialize", BindingFlags.Public | BindingFlags.Static);
        MethodInfo? deserialize = jsonType?.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);

        string? json = serialize?.Invoke(null, [expected]) as string;
        AppSettings? actual = deserialize?.Invoke(null, [json]) as AppSettings;

        Assert.Equal(expected, actual);
    }
}
