namespace OcrTool.Core;

public sealed record AppSettings
{
    public static AppSettings Default { get; } = new();

    public bool ShowResultWindow { get; init; } = true;

    public bool AutoCopy { get; init; } = true;

    public bool StartWithWindows { get; init; }

    public HotkeyGesture Hotkey { get; init; } = HotkeyGesture.Default;
}
