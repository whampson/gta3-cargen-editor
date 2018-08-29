using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Resources;
using WHampson.Gta3CarGenEditor.Views;

namespace WHampson.Gta3CarGenEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
                string.Format("{0}\n\n{1}: {2}\n\n{3}",
                    Strings.DialogMessageUnhandledException1,
                    e.Exception.GetType().Name,
                    e.Exception.Message,
                    string.Format(Strings.DialogMessageUnhandledException2, Strings.AppAuthorContact)),
                Strings.DialogTitleUnhandledException,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Kill the process to bypass shutdown hooks
            Process.GetCurrentProcess().Kill();
        }
    }
}
