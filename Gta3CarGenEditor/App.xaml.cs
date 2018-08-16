using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Models;
using WHampson.Gta3CarGenEditor.ViewModels;

namespace WHampson.Gta3CarGenEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //MainWindow = new MainWindow(new MainViewModel());
            //MainWindow.Show();

            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != true) {
                return;
            }

            SaveDataFile saveData = SaveDataFile.Load(dialog.FileName);

            byte[] newData = SerializableObject.Serialize(saveData);
            File.WriteAllBytes(dialog.FileName + ".dmp", newData);

            Application.Current.Shutdown();
        }
    }
}
