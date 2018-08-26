using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WHampson.Gta3CarGenEditor.Events;
using WHampson.Gta3CarGenEditor.ViewModels;

namespace WHampson.Gta3CarGenEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel.MessageBoxRequested += ViewModel_MessageBoxRequested;
            ViewModel.FileDialogRequested += ViewModel_FileDialogRequested;
            ViewModel.EditMetadataDialogRequested += ViewModel_EditMetadataDialogRequested;
            ViewModel.ResetRowOrderRequested += ViewModel_ResetRowOrderRequested;
        }

        public MainViewModel ViewModel
        {
            get { return (MainViewModel) DataContext; }
            set { DataContext = value; }
        }

        private void ViewModel_ResetRowOrderRequested(object sender, EventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            if (view != null) {
                view.SortDescriptions.Clear();
                foreach (DataGridColumn column in dataGrid.Columns) {
                    column.SortDirection = null;
                }
            }
        }

        private void ViewModel_MessageBoxRequested(object sender, MessageBoxEventArgs e)
        {
            e.Show(this);
        }

        private void ViewModel_FileDialogRequested(object sender, FileDialogEventArgs e)
        {
            e.ShowDialog(this);
        }

        private void ViewModel_EditMetadataDialogRequested(object sender, MetadataDialogEventArgs e)
        {
            MetadataWindow w = new MetadataWindow(new MetadataViewModel(e.Metadata))
            {
                Owner = this
            };
            w.ShowDialog();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Close current file
            if (ViewModel.IsFileOpen) {
                ViewModel.FileCloseCommand.Execute(null);
            }

            // Only close window if user didn't cancel closing file
            if (ViewModel.IsFileOpen) {
                e.Cancel = true;
            }
        }
    }
}
