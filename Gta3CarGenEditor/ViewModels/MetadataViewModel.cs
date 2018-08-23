using System;
using System.Windows.Input;
using WHampson.Gta3CarGenEditor.Events;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Models;

namespace WHampson.Gta3CarGenEditor.ViewModels
{
    public class MetadataViewModel : ObservableObject
    {
        private CarGeneratorsInfo m_metadata;

        public MetadataViewModel(CarGeneratorsInfo metadata)
        {
            m_metadata = metadata;
        }

        public CarGeneratorsInfo Metadata
        {
            get { return m_metadata; }
            set { m_metadata = value; OnPropertyChanged(); }
        }

        public ICommand OkCommand
        {
            get { return new RelayCommand(() => OnDialogCloseRequested(new DialogCloseEventArgs(true))); }
        }

        public ICommand CancelCommand
        {
            get { return new RelayCommand(() => OnDialogCloseRequested(new DialogCloseEventArgs(false))); }
        }

        public event EventHandler<DialogCloseEventArgs> DialogCloseRequested;
        protected void OnDialogCloseRequested(DialogCloseEventArgs e)
        {
            DialogCloseRequested?.Invoke(this, e);
        }
    }
}
