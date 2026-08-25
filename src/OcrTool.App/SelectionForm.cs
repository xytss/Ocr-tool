using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OcrTool.Core;

namespace OcrTool.App;

internal sealed class SelectionForm : Form
{
    private readonly Bitmap _screen;
    private readonly HighContrastCrosshairCursor _crosshairCursor;
    private Point _start;
    private Point _current;
    private bool _dragging;
    private Bitmap? _selection;

    public SelectionForm()
    {
        Rectangle virtualScreen = SystemInformation.VirtualScreen;
        _screen = new Bitmap(
            virtualScreen.Width,
            virtualScreen.Height,
            PixelFormat.Format32bppPArgb);

        using (Graphics graphics = Graphics.FromImage(_screen))
        {
            graphics.CopyFromScreen(virtualScreen.Location, Point.Empty, virtualScreen.Size);
        }

        Bounds = virtualScreen;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        _crosshairCursor = HighContrastCrosshairCursor.Create();
        Cursor = _crosshairCursor.Cursor;
        KeyPreview = true;
        DoubleBuffered = true;
    }

    public Bitmap? SelectRegion()
    {
        ShowDialog();
        return _selection;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            Close();
            return;
        }

        if (e.Button == MouseButtons.Left)
        {
            _start = e.Location;
            _current = e.Location;
            _dragging = true;
            Invalidate();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _current = e.Location;

        if (_dragging)
        {
            Capture = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!_dragging || e.Button != MouseButtons.Left)
        {
            return;
        }

        _current = e.Location;
        _dragging = false;
        Capture = false;
        Rectangle rectangle = CurrentRectangle();

        if (rectangle.Width > 0 && rectangle.Height > 0)
        {
            _selection = _screen.Clone(rectangle, PixelFormat.Format32bppPArgb);
        }

        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
    }

    protected override void OnShown(EventArgs e)
    {
        Activate();
        base.OnShown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.DrawImageUnscaled(_screen, Point.Empty);

        using var shade = new SolidBrush(Color.FromArgb(82, 15, 23, 42));
        e.Graphics.FillRectangle(shade, ClientRectangle);

        Rectangle rectangle = _dragging ? CurrentRectangle() : Rectangle.Empty;
        if (rectangle.Width > 0 && rectangle.Height > 0)
        {
            e.Graphics.SetClip(rectangle);
            e.Graphics.DrawImageUnscaled(_screen, Point.Empty);
            e.Graphics.ResetClip();

            using var border = new Pen(Color.FromArgb(13, 148, 136), 2F)
            {
                Alignment = PenAlignment.Inset
            };
            e.Graphics.DrawRectangle(border, rectangle);
            DrawSelectionSize(e.Graphics, rectangle);
        }

        DrawInstruction(e.Graphics);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Cursor = Cursors.Default;
            _crosshairCursor.Dispose();
            _screen.Dispose();
        }

        base.Dispose(disposing);
    }

    private Rectangle CurrentRectangle()
    {
        SelectionRectangle rectangle = SelectionRectangle.FromPoints(
            _start.X,
            _start.Y,
            _current.X,
            _current.Y);

        return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    private void DrawInstruction(Graphics graphics)
    {
        const string text = "拖动选择文字区域    Esc 或右键取消";
        float scale = graphics.DpiY / 96F;
        using var font = new Font(
            "Microsoft YaHei UI",
            InstructionFontSize(graphics.DpiY),
            FontStyle.Bold,
            GraphicsUnit.Pixel);
        SizeF size = graphics.MeasureString(text, font);
        float height = Math.Max(46F * scale, size.Height + 16F * scale);
        var background = new RectangleF(
            (ClientSize.Width - size.Width) / 2F - 18F * scale,
            20F * scale,
            size.Width + 36F * scale,
            height);
        using var path = RoundedRectangle(background, 9F * scale);
        using var brush = new SolidBrush(Color.FromArgb(220, 23, 33, 31));
        using var textBrush = new SolidBrush(Color.White);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        graphics.FillPath(brush, path);
        graphics.DrawString(text, font, textBrush, background, format);
    }

    private static float InstructionFontSize(float dpi)
    {
        return 16F * dpi / 96F;
    }

    private void DrawSelectionSize(Graphics graphics, Rectangle rectangle)
    {
        string text = $"{rectangle.Width} × {rectangle.Height}";
        using var font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Pixel);
        SizeF size = graphics.MeasureString(text, font);
        float x = Math.Clamp(rectangle.Left, 8F, ClientSize.Width - size.Width - 20F);
        float y = rectangle.Top >= 34
            ? rectangle.Top - 28F
            : Math.Min(rectangle.Bottom + 8F, ClientSize.Height - 28F);
        var background = new RectangleF(x, y, size.Width + 12F, 22F);
        using var brush = new SolidBrush(Color.FromArgb(225, 23, 33, 31));
        using var textBrush = new SolidBrush(Color.White);
        graphics.FillRectangle(brush, background);
        graphics.DrawString(text, font, textBrush, x + 6F, y + 5F);
    }

    private static GraphicsPath RoundedRectangle(RectangleF rectangle, float radius)
    {
        float diameter = radius * 2F;
        var path = new GraphicsPath();
        path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180F, 90F);
        path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270F, 90F);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0F, 90F);
        path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90F, 90F);
        path.CloseFigure();
        return path;
    }
}
