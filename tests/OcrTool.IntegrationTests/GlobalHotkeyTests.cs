using OcrTool.Core;
using Xunit;

namespace OcrTool.App.Tests;

public sealed class GlobalHotkeyTests
{
    [Fact]
    public void Global_hotkey_exposes_runtime_change_API()
    {
        Type type = typeof(App).Assembly.GetType("OcrTool.App.GlobalHotkey")!;
        Type[] parameters = [typeof(HotkeyGesture)];

        Assert.NotNull(type.GetMethod("TryChange", parameters));
    }
}
