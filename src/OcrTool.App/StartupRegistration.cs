using Microsoft.Win32;

namespace OcrTool.App;

internal sealed class StartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly string _valueName;
    private readonly string _command;

    public StartupRegistration(string valueName, string executablePath)
    {
        _valueName = valueName;
        _command = CommandForExecutable(executablePath);
    }

    public void SetEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)!;

        if (enabled)
        {
            key.SetValue(_valueName, _command, RegistryValueKind.String);
        }
        else
        {
            key.DeleteValue(_valueName, throwOnMissingValue: false);
        }
    }

    private static string CommandForExecutable(string executablePath)
    {
        return $"\"{executablePath}\"";
    }
}
