using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OcrTool.App;
using OcrTool.Core;
using Xunit;
using WpfButton = System.Windows.Controls.Button;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace OcrTool.IntegrationTests;

public sealed class SettingsWindowTests
{
    [Fact]
    public void Settings_window_uses_readable_typography()
    {
        RunOnStaThread(() =>
        {
            var window = CreateOffscreenSettingsWindow();
            window.Show();
            window.UpdateLayout();

            Assert.Equal("Microsoft YaHei UI", window.FontFamily.Source);
            Assert.Equal(15D, window.FontSize);
            Assert.Equal(24D, FindText(window, "设置").FontSize);
            Assert.Equal(14D, FindText(window, "快捷键、识别行为与系统启动设置").FontSize);
            Assert.Equal(16D, FindText(window, "截图快捷键").FontSize);
            Assert.Equal(14D, ((TextBlock)window.FindName("HotkeyHint")).FontSize);
            Assert.Equal(16D, FindText(window, "识别与启动行为").FontSize);
            Assert.All(
                FindVisualChildren<WpfCheckBox>(window),
                checkbox => Assert.Equal(16D, checkbox.FontSize));
            Assert.All(
                FindVisualChildren<WpfButton>(window),
                button => Assert.Equal(15D, button.FontSize));

            window.Close();
        });
    }

    [Fact]
    public void Result_window_uses_readable_typography()
    {
        RunOnStaThread(() =>
        {
            using var screenshot = new System.Drawing.Bitmap(10, 10);
            var window = new OcrResultWindow(screenshot)
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false
            };
            window.Show();
            window.UpdateLayout();

            Assert.Equal("Microsoft YaHei UI", window.FontFamily.Source);
            Assert.Equal(15D, window.FontSize);
            Assert.Equal(24D, FindText(window, "OCR 结果").FontSize);
            Assert.Equal(14D, ((TextBlock)window.FindName("StatusText")).FontSize);
            Assert.Equal(14D, FindText(window, "Ctrl+Enter 复制并关闭").FontSize);
            Assert.Equal(14D, FindText(window, "原图").FontSize);
            Assert.Equal(18D, ((WpfTextBox)window.FindName("ResultTextBox")).FontSize);
            Assert.Equal(14D, ((TextBlock)window.FindName("EmptyText")).FontSize);
            Assert.All(
                FindVisualChildren<WpfButton>(window),
                button => Assert.Equal(15D, button.FontSize));

            window.Close();
        });
    }

    [Fact]
    public void Startup_checkbox_is_fully_visible_above_the_footer()
    {
        RunOnStaThread(() =>
        {
            var window = CreateOffscreenSettingsWindow();
            window.Height = 480;
            window.Show();
            window.UpdateLayout();

            var checkbox = (FrameworkElement)window.FindName("StartWithWindowsCheckBox");
            var root = (Grid)window.Content;
            FrameworkElement footer = root.Children
                .OfType<FrameworkElement>()
                .Single(element => Grid.GetRow(element) == 2);
            double checkboxBottom = checkbox
                .TranslatePoint(new System.Windows.Point(0, checkbox.ActualHeight), root)
                .Y;
            double footerTop = footer.TranslatePoint(new System.Windows.Point(0, 0), root).Y;

            Assert.True(
                checkbox.ActualHeight > 0 && checkboxBottom <= footerTop,
                "开机启动开关被底部按钮栏裁切。");
            window.Close();
        });
    }

    [Fact]
    public void Settings_window_can_show_startup_hotkey_conflict()
    {
        RunOnStaThread(() =>
        {
            var window = CreateOffscreenSettingsWindow();
            const string message = "快捷键 F2 已被其他程序占用，请设置新的快捷键。";
            var showError = typeof(SettingsWindow).GetMethod("ShowError");

            Assert.NotNull(showError);
            showError!.Invoke(window, [message]);

            var hint = (TextBlock)window.FindName("HotkeyHint");
            Assert.Equal(message, hint.Text);
            Assert.Equal(
                System.Windows.Media.Color.FromRgb(185, 28, 28),
                ((SolidColorBrush)hint.Foreground).Color);
            window.Close();
        });
    }

    private static SettingsWindow CreateOffscreenSettingsWindow()
    {
        return new SettingsWindow(AppSettings.Default, _ => null)
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -10000,
            Top = -10000,
            ShowInTaskbar = false
        };
    }

    private static TextBlock FindText(DependencyObject root, string text)
    {
        return FindVisualChildren<TextBlock>(root).Single(element => element.Text == text);
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (T descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private static void RunOnStaThread(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
    }
}
