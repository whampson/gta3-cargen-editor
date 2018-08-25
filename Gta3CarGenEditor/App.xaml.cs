using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Views;

namespace WHampson.Gta3CarGenEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        const int FatalErrorExitCode = 127;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            MessageBoxEx.Show(
                MainWindow,
                string.Format("A fatal exception has occurred. The application will be terminated.\n\n{0}: {1}\n\nPlease contact thehambone93@gmail.com about this error.",
                    e.Exception.GetType().Name, e.Exception.Message),
                "Unhandled Exception",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Kill the process to bypass shutdown hooks
            Process.GetCurrentProcess().Kill();
        }
    }
}
