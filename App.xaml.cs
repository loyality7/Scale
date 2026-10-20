using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ScaleWindows;

/// <summary>
/// Interaction logic for App.xaml with global crash resilience.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogCrash(e.Exception, "DispatcherUnhandledException");
        MessageBox.Show(
            $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nA diagnostic report has been logged to crash.log.",
            "Scale - Application Notice",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        e.Handled = true; // Prevent process abort
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogCrash(ex, "CurrentDomain_UnhandledException");
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogCrash(e.Exception, "TaskScheduler_UnobservedTaskException");
        e.SetObserved();
    }

    private static void LogCrash(Exception ex, string context)
    {
        try
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{context}]\n{ex}\n----------------------------------------\n";
            File.AppendAllText(logPath, logEntry);
        }
        catch
        {
            // Suppress secondary logging failures
        }
    }
}

