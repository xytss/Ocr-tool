using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OcrTool.App;

internal sealed class HighContrastCrosshairCursor : IDisposable
{
    private IntPtr _handle;

    private HighContrastCrosshairCursor(IntPtr handle)
    {
        _handle = handle;
        Cursor = new Cursor(handle);
    }

    public Cursor Cursor { get; }

    public static HighContrastCrosshairCursor Create()
    {
        const int size = 48;
        const int center = size / 2;
        using var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var outline = new Pen(Color.FromArgb(15, 23, 42), 7F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            using var foreground = new Pen(Color.White, 3F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            DrawCrosshair(graphics, outline, center);
            DrawCrosshair(graphics, foreground, center);

            graphics.FillEllipse(Brushes.White, center - 7, center - 7, 14, 14);
            using var accent = new SolidBrush(Color.FromArgb(13, 148, 136));
            graphics.FillEllipse(accent, center - 4, center - 4, 8, 8);
        }

        IntPtr iconHandle = bitmap.GetHicon();
        GetIconInfo(iconHandle, out IconInfo iconInfo);
        iconInfo.IsIcon = false;
        iconInfo.HotspotX = center;
        iconInfo.HotspotY = center;
        IntPtr cursorHandle = CreateIconIndirect(ref iconInfo);

        DeleteObject(iconInfo.MaskBitmap);
        DeleteObject(iconInfo.ColorBitmap);
        DestroyIcon(iconHandle);

        return new HighContrastCrosshairCursor(cursorHandle);
    }

    public void Dispose()
    {
        Cursor.Dispose();
        DestroyCursor(_handle);
        _handle = IntPtr.Zero;
    }

    private static void DrawCrosshair(Graphics graphics, Pen pen, int center)
    {
        const int edge = 4;
        const int gap = 9;
        int farEdge = (center * 2) - edge - 1;

        graphics.DrawLine(pen, edge, center, center - gap, center);
        graphics.DrawLine(pen, center + gap, center, farEdge, center);
        graphics.DrawLine(pen, center, edge, center, center - gap);
        graphics.DrawLine(pen, center, center + gap, center, farEdge);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IconInfo
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool IsIcon;

        public int HotspotX;
        public int HotspotY;
        public IntPtr MaskBitmap;
        public IntPtr ColorBitmap;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetIconInfo(IntPtr icon, out IconInfo iconInfo);

    [DllImport("user32.dll")]
    private static extern IntPtr CreateIconIndirect(ref IconInfo iconInfo);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr icon);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyCursor(IntPtr cursor);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr graphicsObject);
}
