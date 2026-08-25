using System.IO;
using System.Windows;
using OcrTool.Core;

namespace OcrTool.App;

public partial class App : System.Windows.Application
{
    private GlobalHotkey? _hotkey;
    private TrayIcon? _tray;
    private Task<OcrEngine>? _engineTask;
    private AppSettingsStore? _settingsStore;
    private StartupRegistration? _startupRegistration;
    private AppSettings _settings = AppSettings.Default;
    private OcrResultWindow? _resultWindow;
    private SettingsWindow? _settingsWindow;
    private bool _isBusy;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        string settingsPath = AppStoragePath.Resolve(
            AppContext.BaseDirectory,
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            File.Exists(Path.Combine(AppContext.BaseDirectory, "portable.flag")));
        _settingsStore = new AppSettingsStore(settingsPath);
        _settings = _settingsStore.Load();
        _startupRegistration = new StartupRegistration("OcrTool", Environment.ProcessPath!);
        _tray = new TrayIcon(BeginOcr, ShowSettings, Shutdown, _settings.Hotkey.DisplayText);
        _hotkey = new GlobalHotkey(BeginOcr, _settings.Hotkey);
        _engineTask = Task.Run(static () => new OcrEngine());

        if (!_hotkey.IsRegistered)
        {
            _tray.Notify("快捷键已被占用，请从托盘打开设置并修改。");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkey?.Dispose();
        _tray?.Dispose();

        if (_engineTask is { IsCompletedSuccessfully: true })
        {
            _engineTask.Result.Dispose();
        }

        base.OnExit(e);
    }

    private async void BeginOcr()
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;

        try
        {
            using var selector = new SelectionForm();
            using System.Drawing.Bitmap? image = selector.SelectRegion();

            if (image is null)
            {
                return;
            }

            OcrResultWindow? resultWindow = _settings.ShowResultWindow
                ? ShowResultWindow(image)
                : null;

            OcrEngine engine = await _engineTask!;
            string text = await Task.Run(() => engine.Recognize(image));
            bool copied = false;

            if (!string.IsNullOrWhiteSpace(text) && _settings.AutoCopy)
            {
                System.Windows.Clipboard.SetText(text);
                copied = true;
            }

            if (resultWindow?.IsLoaded == true)
            {
                resultWindow.Complete(text, copied);
            }
            else if (string.IsNullOrWhiteSpace(text))
            {
                _tray!.Notify("未识别到文字");
            }
            else if (copied)
            {
                _tray!.Notify("文字已复制到剪贴板");
            }
        }
        catch (Exception exception)
        {
            if (_resultWindow?.IsLoaded == true)
            {
                _resultWindow.Fail(exception.Message);
            }
            else
            {
                _tray!.Notify($"识别失败：{exception.Message}");
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    private OcrResultWindow ShowResultWindow(System.Drawing.Bitmap image)
    {
        if (_resultWindow is null)
        {
            _resultWindow = new OcrResultWindow(image);
            _resultWindow.Closed += (_, _) => _resultWindow = null;
            _resultWindow.Show();
        }
        else
        {
            if (_resultWindow.WindowState == WindowState.Minimized)
            {
                _resultWindow.WindowState = WindowState.Normal;
            }

            _resultWindow.Reset(image);
        }

        _resultWindow.Activate();
        return _resultWindow;
    }

    private void ShowSettings()
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_settings, ApplySettings);
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.ShowDialog();
    }

    private string? ApplySettings(AppSettings settings)
    {
        if (!settings.ShowResultWindow && !settings.AutoCopy)
        {
            return "请至少保留一种结果输出方式。";
        }

        AppSettings previous = _settings;
        GlobalHotkey hotkey = _hotkey!;
        bool hotkeyChanged = settings.Hotkey != previous.Hotkey;
        bool hotkeyRegistrationRequired = hotkeyChanged || !hotkey.IsRegistered;
        bool startupChanged = settings.StartWithWindows != previous.StartWithWindows;

        if (hotkeyRegistrationRequired && !hotkey.TryChange(settings.Hotkey))
        {
            return $"快捷键 {settings.Hotkey.DisplayText} 已被其他程序占用。";
        }

        try
        {
            if (startupChanged)
            {
                _startupRegistration!.SetEnabled(settings.StartWithWindows);
            }
        }
        catch (Exception exception)
        {
            if (hotkeyChanged)
            {
                hotkey.TryChange(previous.Hotkey);
            }

            return $"开机启动设置无法保存：{exception.Message}";
        }

        try
        {
            _settingsStore!.Save(settings);
        }
        catch (Exception exception)
        {
            if (hotkeyChanged)
            {
                hotkey.TryChange(previous.Hotkey);
            }

            if (startupChanged)
            {
                _startupRegistration!.SetEnabled(previous.StartWithWindows);
            }

            return $"设置无法保存：{exception.Message}";
        }

        _settings = settings;
        _tray!.UpdateHotkey(settings.Hotkey.DisplayText);
        return null;
    }
}
