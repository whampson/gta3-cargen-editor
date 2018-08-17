using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Models;

namespace WHampson.Gta3CarGenEditor.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private SaveDataFile _saveDataFile;
        private string _saveDataFilePath;
        private ObservableCollection<CarGenerator> _carGens;
        private bool _showUnused;

        public MainViewModel()
        {
            CarGenerators = new ObservableCollection<CarGenerator>();
            CarGenerators.CollectionChanged += CarGenerators_CollectionChanged;
        }

        public SaveDataFile SaveDataFile
        {
            get {
                return _saveDataFile;
            }

            set {
                _saveDataFile = value;
                if (_saveDataFile == null) {
                    CarGenerators = null;
                }
                else {
                    foreach (CarGenerator cg in SaveDataFile.CarGenerators.CarGeneratorsArray) {
                        CarGenerators.Add(cg);
                    }
                }

                OnPropertyChanged();
            }
        }

        private void CarGenerators_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null) {
                foreach (CarGenerator cg in e.NewItems) {
                    cg.PropertyChanged += Cg_PropertyChanged;
                }
            }

            if (e.OldItems != null) {
                foreach (CarGenerator cg in e.OldItems) {
                    cg.PropertyChanged -= Cg_PropertyChanged;
                }
            }
        }

        private void Cg_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(null, "Something changed!"));
        }

        public string SaveDataFilePath
        {
            get { return _saveDataFilePath; }
            set { _saveDataFilePath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CarGenerator> CarGenerators
        {
            get { return _carGens; }
            set { _carGens = value; OnPropertyChanged(); }
        }

        public bool IsEditingFile
        {
            get { return _saveDataFile != null; }
        }

        public bool ShowUnused
        {
            get { return _showUnused; }
            set { _showUnused = value; OnPropertyChanged(); }
        }

        #region Commands
        public ICommand OpenFile
        {
            get {
                return new RelayCommand(OpenFile_Execute);
            }
        }

        private void OpenFile_Execute()
        {
            // TODO: this should be invoked by the view
            OpenFileDialog diag = new OpenFileDialog()
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "All Files (*.*)|*.*"
            };

            bool? fileSelected = diag.ShowDialog();
            if (fileSelected !=true) {
                return;
            }

            SaveDataFile = SaveDataFile.Load(diag.FileName);
            SaveDataFilePath = diag.FileName;
        }

        public ICommand CloseFile
        {
            get {
                return new RelayCommand(CloseFile_Execute, CloseFile_CanExecute);
            }
        }

        private bool CloseFile_CanExecute()
        {
            return IsEditingFile;
        }

        private void CloseFile_Execute()
        {
            // TODO: prompt user to save
            SaveDataFile = null;
        }

        public ICommand SaveFile
        {
            get {
                return new RelayCommand(SaveFile_Execute, SaveFile_CanExecute);
            }
        }

        private bool SaveFile_CanExecute()
        {
            return IsEditingFile;
        }

        private void SaveFile_Execute()
        {
            uint numCarGens = 0;
            foreach (CarGenerator cg in CarGenerators) {
                if (cg.Model != VehicleModel.None) {
                    numCarGens++;
                }
            }

            SaveDataFile.CarGenerators.CarGeneratorsInfo.NumberOfCarGenerators = numCarGens;
            SaveDataFile.CarGenerators.CarGeneratorsArray = CarGenerators.ToArray();
            SaveDataFile.Store(SaveDataFilePath);
        }

        public ICommand SaveFileAs
        {
            get {
                return new RelayCommand(SaveFileAs_Execute, SaveFileAs_CanExecute);
            }
        }

        private bool SaveFileAs_CanExecute()
        {
            return IsEditingFile;
        }

        private void SaveFileAs_Execute()
        {
            // TODO: this should be invoked by the view
            SaveFileDialog diag = new SaveFileDialog
            {
                Filter = "All Files (*.*)|*.*"
            };

            bool? fileSelected = diag.ShowDialog();
            if (fileSelected != true) {
                return;
            }

            SaveDataFilePath = diag.FileName;
            SaveFile.Execute(null);
        }

        public ICommand ExitApplication
        {
            get {
                return new RelayCommand(ExitApplication_Execute);
            }
        }

        private void ExitApplication_Execute()
        {
            Application.Current.MainWindow.Close();
        }

        public ICommand AboutApplication
        {
            get {
                return new RelayCommand(AboutApplication_Execute);
            }
        }

        private void AboutApplication_Execute()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(null, "AboutApplication"));
        }
        #endregion

        public event EventHandler<MessageBoxEventArgs> MessageBoxRequested;

        protected void OnMessageBoxRequested(MessageBoxEventArgs e)
        {
            MessageBoxRequested?.Invoke(this, e);
        }
    }
}
