using System.Windows;
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
            // TODO: uncaught exception handler
            // TODO: handle I/O exceptions with dialog

            MainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
