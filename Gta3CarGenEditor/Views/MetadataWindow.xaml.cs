using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox txt) {
                txt.SelectAll();
            }
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox txt) {
                if (!txt.IsKeyboardFocusWithin) {
                    e.Handled = true;
                    txt.Focus();
                }
            }
        }
    }
}
