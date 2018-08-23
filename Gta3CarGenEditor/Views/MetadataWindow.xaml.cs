using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WHampson.Gta3CarGenEditor.Events;
using WHampson.Gta3CarGenEditor.ViewModels;

namespace WHampson.Gta3CarGenEditor.Views
{
    /// <summary>
    /// Interaction logic for MetadataWindow.xaml
    /// </summary>
    public partial class MetadataWindow : Window
    {
        public MetadataWindow(MetadataViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.DialogCloseRequested += ViewModel_DialogCloseRequested;

            InitializeComponent();
        }

        public MetadataViewModel ViewModel
        {
            get { return (MetadataViewModel) DataContext; }
            set { DataContext = value; }
        }

        private void UpdateBindingSources()
        {
            txtTotalCount.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtActiveCount.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtProcessCount.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtIsCloseCount.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void ViewModel_DialogCloseRequested(object sender, DialogCloseEventArgs e)
        {
            DialogResult = e.DialogResult;

            if (DialogResult == true) {
                UpdateBindingSources();
            }
            Close();
        }
    }
}
