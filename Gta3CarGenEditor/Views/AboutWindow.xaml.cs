using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace WHampson.Gta3CarGenEditor.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            ViewModel.DialogCloseRequested += ViewModel_DialogCloseRequested;
            ViewModel.NavigationRequested += Hyperlink_RequestNavigate;
        }

        public AboutViewModel ViewModel
        {
            get { return (AboutViewModel) DataContext; }
            set { DataContext = value; }
        }

        private void ViewModel_DialogCloseRequested(object sender, Events.DialogCloseEventArgs e)
        {
            DialogResult = e.DialogResult;
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri != null) {
                Process.Start(e.Uri.ToString());
            }
        }
    }
}
