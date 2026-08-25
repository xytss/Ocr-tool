using System.Windows;
using System.Windows.Input;
using OcrTool.Core;

namespace OcrTool.App;

public partial class SettingsWindow : Window
{
    private readonly Func<AppSettings, string?> _apply;
    private HotkeyGesture _hotkey;
    private bool _recording;

    public SettingsWindow(AppSettings settings, Func<AppSettings, string?> apply)
    {
        InitializeComponent();
        _apply = apply;
        _hotkey = settings.Hotkey;
        HotkeyButton.Content = _hotkey.DisplayText;
        ShowResultCheckBox.IsChecked = settings.ShowResultWindow;
        AutoCopyCheckBox.IsChecked = settings.AutoCopy;
        StartWithWindowsCheckBox.IsChecked = settings.StartWithWindows;
    }

    public void ShowError(string message)
    {
        HotkeyHint.Text = message;
        HotkeyHint.Foreground = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(185, 28, 28));
    }

    private void HotkeyButton_Click(object sender, RoutedEventArgs e)
    {
        _recording = true;
        HotkeyButton.Content = "请按下新的组合键…";
        HotkeyHint.Text = "F1–F24 可单独使用；其他按键需要搭配修饰键。";
        HotkeyHint.Foreground = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(82, 97, 93));
        HotkeyButton.Focus();
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!_recording)
        {
            return;
        }

        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (IsModifierKey(key))
        {
            e.Handled = true;
            return;
        }

        ModifierKeys modifiers = Keyboard.Modifiers;
        var hotkey = new HotkeyGesture(
            ToHotkeyModifiers(modifiers),
            KeyInterop.VirtualKeyFromKey(key),
            KeyName(key));

        if (modifiers == ModifierKeys.None && !hotkey.CanUseWithoutModifiers)
        {
            HotkeyHint.Text = "字母和数字快捷键需要至少同时按下一个修饰键。";
            HotkeyHint.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(185, 28, 28));
            e.Handled = true;
            return;
        }

        _hotkey = hotkey;
        _recording = false;
        HotkeyButton.Content = _hotkey.DisplayText;
        HotkeyHint.Text = "快捷键已录入，保存后生效。";
        HotkeyHint.Foreground = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(13, 148, 136));
        e.Handled = true;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = new AppSettings
        {
            Hotkey = _hotkey,
            ShowResultWindow = ShowResultCheckBox.IsChecked == true,
            AutoCopy = AutoCopyCheckBox.IsChecked == true,
            StartWithWindows = StartWithWindowsCheckBox.IsChecked == true
        };
        string? error = _apply(settings);

        if (error is not null)
        {
            ShowError(error);
            return;
        }

        DialogResult = true;
    }

    private static bool IsModifierKey(Key key)
    {
        return key is Key.LeftCtrl
            or Key.RightCtrl
            or Key.LeftAlt
            or Key.RightAlt
            or Key.LeftShift
            or Key.RightShift
            or Key.LWin
            or Key.RWin;
    }

    private static HotkeyModifiers ToHotkeyModifiers(ModifierKeys modifiers)
    {
        HotkeyModifiers result = 0;

        if (modifiers.HasFlag(ModifierKeys.Control))
        {
            result |= HotkeyModifiers.Control;
        }

        if (modifiers.HasFlag(ModifierKeys.Alt))
        {
            result |= HotkeyModifiers.Alt;
        }

        if (modifiers.HasFlag(ModifierKeys.Shift))
        {
            result |= HotkeyModifiers.Shift;
        }

        if (modifiers.HasFlag(ModifierKeys.Windows))
        {
            result |= HotkeyModifiers.Windows;
        }

        return result;
    }

    private static string KeyName(Key key)
    {
        return key switch
        {
            >= Key.D0 and <= Key.D9 => ((int)key - (int)Key.D0).ToString(),
            Key.OemPlus => "+",
            Key.OemMinus => "-",
            Key.OemComma => ",",
            Key.OemPeriod => ".",
            Key.OemQuestion => "/",
            _ => key.ToString()
        };
    }
}
