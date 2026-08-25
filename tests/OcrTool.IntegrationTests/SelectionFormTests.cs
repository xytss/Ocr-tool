using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using OcrTool.App;
using Xunit;

namespace OcrTool.IntegrationTests;

public sealed class SelectionFormTests
{
    [Theory]
    [InlineData(96F, 16F)]
    [InlineData(144F, 24F)]
    public void Instruction_font_size_scales_with_display_dpi(float dpi, float expectedSize)
    {
        MethodInfo? method = typeof(SelectionForm).GetMethod(
            "InstructionFontSize",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(method);
        Assert.Equal(expectedSize, (float)method.Invoke(null, [dpi])!);
    }

    [Fact]
    public void Selection_form_uses_custom_crosshair_cursor()
    {
        using var form = new SelectionForm();

        Assert.NotSame(Cursors.Cross, form.Cursor);
    }

    [Fact]
    public void Selection_cursor_is_at_least_48_pixels_wide()
    {
        using var form = new SelectionForm();
        GetIconInfo(form.Cursor.Handle, out IconInfo iconInfo);

        try
        {
            GetObject(iconInfo.ColorBitmap, Marshal.SizeOf<NativeBitmap>(), out NativeBitmap bitmap);
            Assert.True(bitmap.Width >= 48, $"实际宽度：{bitmap.Width}");
        }
        finally
        {
            DeleteObject(iconInfo.MaskBitmap);
            DeleteObject(iconInfo.ColorBitmap);
        }
    }

    [Fact]
    public void Selection_cursor_contains_teal_center_highlight()
    {
        using var form = new SelectionForm();
        using var image = new Bitmap(form.Cursor.Size.Width, form.Cursor.Size.Height);
        using Graphics graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Transparent);
        form.Cursor.Draw(graphics, new Rectangle(Point.Empty, form.Cursor.Size));

        bool containsTeal = false;
        for (int y = 0; y < image.Height && !containsTeal; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                if (pixel.R == 13 && pixel.G == 148 && pixel.B == 136)
                {
                    containsTeal = true;
                    break;
                }
            }
        }

        Assert.True(containsTeal);
    }

    [Fact]
    public void Selection_form_has_no_software_pointer_renderer()
    {
        MethodInfo? drawPointer = typeof(SelectionForm).GetMethod(
            "DrawPointer",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        Assert.Null(drawPointer);
    }

    [Fact]
    public void Pointer_move_without_dragging_does_not_invalidate_overlay()
    {
        using var form = new SelectionForm();
        _ = form.Handle;
        int invalidations = 0;
        form.Invalidated += (_, _) => invalidations++;

        MethodInfo onMouseMove = typeof(SelectionForm).GetMethod(
            "OnMouseMove",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!;
        onMouseMove.Invoke(form, [new MouseEventArgs(MouseButtons.None, 0, 100, 100, 0)]);

        Assert.Equal(0, invalidations);
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

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeBitmap
    {
        public int Type;
        public int Width;
        public int Height;
        public int WidthBytes;
        public ushort Planes;
        public ushort BitsPerPixel;
        public IntPtr Bits;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetIconInfo(IntPtr icon, out IconInfo iconInfo);

    [DllImport("gdi32.dll")]
    private static extern int GetObject(IntPtr graphicsObject, int bufferSize, out NativeBitmap bitmap);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr graphicsObject);
}
