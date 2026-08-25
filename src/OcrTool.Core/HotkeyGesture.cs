namespace OcrTool.Core;

[Flags]
public enum HotkeyModifiers : uint
{
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008
}

public readonly record struct HotkeyGesture(
    HotkeyModifiers Modifiers,
    int VirtualKey,
    string KeyName)
{
    public static HotkeyGesture Default { get; } = new(
        0,
        0x71,
        "F2");

    public bool CanUseWithoutModifiers => VirtualKey is >= 0x70 and <= 0x87;

    public string DisplayText
    {
        get
        {
            var parts = new List<string>(5);

            if (Modifiers.HasFlag(HotkeyModifiers.Control))
            {
                parts.Add("Ctrl");
            }

            if (Modifiers.HasFlag(HotkeyModifiers.Alt))
            {
                parts.Add("Alt");
            }

            if (Modifiers.HasFlag(HotkeyModifiers.Shift))
            {
                parts.Add("Shift");
            }

            if (Modifiers.HasFlag(HotkeyModifiers.Windows))
            {
                parts.Add("Win");
            }

            parts.Add(KeyName);
            return string.Join(" + ", parts);
        }
    }
}
