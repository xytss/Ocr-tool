using System.Drawing;
using System.Windows.Forms;

namespace OcrTool.App;

internal sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly Icon _applicationIcon;
    private readonly ToolStripMenuItem _beginOcrItem;

    public TrayIcon(Action beginOcr, Action showSettings, Action exit, string hotkeyText)
    {
        var menu = new ContextMenuStrip();
        _beginOcrItem = new ToolStripMenuItem(string.Empty, null, (_, _) => beginOcr());
        menu.Items.Add(_beginOcrItem);
        menu.Items.Add("设置…", null, (_, _) => showSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => exit());

        _applicationIcon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!)!;
        _icon = new NotifyIcon
        {
            ContextMenuStrip = menu,
            Icon = _applicationIcon,
            Text = "OCR 工具",
            Visible = true
        };
        _icon.DoubleClick += (_, _) => beginOcr();
        UpdateHotkey(hotkeyText);
    }

    public void UpdateHotkey(string hotkeyText)
    {
        _beginOcrItem.Text = $"开始识别    {hotkeyText}";
        _icon.Text = $"OCR 工具 · {hotkeyText}";
    }

    public void Notify(string message)
    {
        _icon.ShowBalloonTip(1500, "OCR 工具", message, ToolTipIcon.Info);
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.ContextMenuStrip?.Dispose();
        _icon.Dispose();
        _applicationIcon.Dispose();
    }
}
