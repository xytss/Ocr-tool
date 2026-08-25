using System.Runtime.InteropServices;
using System.Windows.Interop;
using OcrTool.Core;

namespace OcrTool.App;

internal sealed class GlobalHotkey : IDisposable
{
    private const int HotkeyId = 1;
    private const int WmHotkey = 0x0312;
    private readonly Action _pressed;
    private readonly HwndSource _source;
    private HotkeyGesture _gesture;
    private bool _isRegistered;

    public GlobalHotkey(Action pressed, HotkeyGesture gesture)
    {
        _pressed = pressed;
        _source = new HwndSource(new HwndSourceParameters("OcrToolHotkey")
        {
            ParentWindow = new IntPtr(-3),
            WindowStyle = 0
        });
        _source.AddHook(WindowProcedure);
        _gesture = gesture;
        _isRegistered = Register(_gesture);
    }

    public bool IsRegistered => _isRegistered;

    public void Dispose()
    {
        if (_isRegistered)
        {
            UnregisterHotKey(_source.Handle, HotkeyId);
        }

        _source.RemoveHook(WindowProcedure);
        _source.Dispose();
    }

    public bool TryChange(HotkeyGesture gesture)
    {
        if (_isRegistered && gesture == _gesture)
        {
            return true;
        }

        HotkeyGesture previous = _gesture;
        bool previousWasRegistered = _isRegistered;

        if (_isRegistered)
        {
            UnregisterHotKey(_source.Handle, HotkeyId);
            _isRegistered = false;
        }

        if (Register(gesture))
        {
            _gesture = gesture;
            _isRegistered = true;
            return true;
        }

        if (previousWasRegistered)
        {
            _isRegistered = Register(previous);
        }

        return false;
    }

    private IntPtr WindowProcedure(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (message == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            _pressed();
        }

        return IntPtr.Zero;
    }

    private bool Register(HotkeyGesture gesture)
    {
        return RegisterHotKey(
            _source.Handle,
            HotkeyId,
            (uint)gesture.Modifiers,
            (uint)gesture.VirtualKey);
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
