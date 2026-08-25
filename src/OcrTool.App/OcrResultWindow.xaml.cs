using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace OcrTool.App;

public partial class OcrResultWindow : Window
{
    public OcrResultWindow(Bitmap screenshot)
    {
        InitializeComponent();
        ScreenshotImage.Source = BitmapSourceFactory.Create(screenshot);
    }

    public void Reset(Bitmap screenshot)
    {
        ScreenshotImage.Source = BitmapSourceFactory.Create(screenshot);
        ResultTextBox.Clear();
        ResultTextBox.IsReadOnly = true;
        EmptyText.Text = "正在识别，请稍候…";
        EmptyText.Visibility = Visibility.Visible;
        StatusText.Text = "正在识别…";
        CopyButton.IsEnabled = false;
        Activate();
    }

    public void Complete(string text, bool automaticallyCopied)
    {
        ResultTextBox.Text = text;
        ResultTextBox.IsReadOnly = false;
        EmptyText.Text = string.IsNullOrWhiteSpace(text) ? "未识别到文字，可在此手动输入。" : string.Empty;
        EmptyText.Visibility = string.IsNullOrWhiteSpace(text)
            ? Visibility.Visible
            : Visibility.Collapsed;
        StatusText.Text = string.IsNullOrWhiteSpace(text)
            ? "未识别到文字"
            : automaticallyCopied ? "识别完成 · 已自动复制" : "识别完成";
        ResultTextBox.Focus();
        ResultTextBox.CaretIndex = ResultTextBox.Text.Length;
    }

    public void Fail(string message)
    {
        ResultTextBox.Clear();
        ResultTextBox.IsReadOnly = false;
        EmptyText.Text = $"识别失败：{message}";
        EmptyText.Visibility = Visibility.Visible;
        StatusText.Text = "识别失败";
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        CopyAndClose(closeAfterCopy: false);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResultTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (CopyButton is not null)
        {
            CopyButton.IsEnabled = !string.IsNullOrWhiteSpace(ResultTextBox.Text);
            EmptyText.Visibility = string.IsNullOrWhiteSpace(ResultTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            CopyAndClose(closeAfterCopy: true);
            e.Handled = true;
        }
    }

    private void CopyAndClose(bool closeAfterCopy)
    {
        if (string.IsNullOrWhiteSpace(ResultTextBox.Text))
        {
            return;
        }

        System.Windows.Clipboard.SetText(ResultTextBox.Text);
        StatusText.Text = "已复制编辑后的文字";

        if (closeAfterCopy)
        {
            Close();
        }
    }
}
