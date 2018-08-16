using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WHampson.Gta3CarGenEditor.ViewModels;

namespace WHampson.Gta3CarGenEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel vm)
        {
            ViewModel = vm;
            vm.MessageBoxRequested += ViewModel_MessageBoxRequested;

            InitializeComponent();
        }

        public MainViewModel ViewModel
        {
            get { return (MainViewModel) DataContext; }
            set { DataContext = value; }
        }

        private void ViewModel_MessageBoxRequested(object sender, Helpers.MessageBoxEventArgs e)
        {
            e.Show(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainViewModel vm = (MainViewModel) DataContext;
            if (vm.IsEditingFile) {
                vm.CloseFile.Execute(null);
            }

            // Only close if user didn't cancel
            if (vm.IsEditingFile) {
                e.Cancel = true;
            }
        }
    }
}
