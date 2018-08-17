using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Models;
using WHampson.Gta3CarGenEditor.Properties;
using static WHampson.Gta3CarGenEditor.Helpers.FileDialogEventArgs;

namespace WHampson.Gta3CarGenEditor.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private SaveDataFile m_currentSaveData;
        private ObservableCollection<CarGenerator> m_carGenerators;
        private string m_mostRecentPath;
        private bool m_isShowingUnusedFields;
        private bool m_isFileModified;

        public MainViewModel()
        {
            m_carGenerators = new ObservableCollection<CarGenerator>();
            m_carGenerators.CollectionChanged += CarGenerators_CollectionChanged;
        }

        #region Properties
        public SaveDataFile CurrentSaveData
        {
            get { return m_currentSaveData; }
            set { m_currentSaveData = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CarGenerator> CarGenerators
        {
            get { return m_carGenerators; }
            set { m_carGenerators = value; OnPropertyChanged(); }
        }

        public string MostRecentPath
        {
            get { return m_mostRecentPath; }
            set { m_mostRecentPath = value; OnPropertyChanged(); }
        }

        public bool IsShowingUnusedFields
        {
            get { return m_isShowingUnusedFields; }
            set { m_isShowingUnusedFields = value; OnPropertyChanged(); }
        }

        public bool IsFileOpen
        {
            get { return m_currentSaveData != null; }
        }

        public bool IsFileModified
        {
            get { return m_isFileModified; }
            set { m_isFileModified = value; OnPropertyChanged(); }
        }
        #endregion

        private void FileOpen(string path)
        {
            CurrentSaveData = SaveDataFile.Load(path);
            MostRecentPath = path;
        }

        private void FileSave(string path)
        {
            CurrentSaveData.Store(path);
            MostRecentPath = path;
        }

        private void FileClose()
        {
            if (IsFileModified) {
                ShowFileClosePrompt();
            }
            else {
                DoFileClose();
            }
        }

        private void DoFileClose()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs("Closing file!!"));
            CurrentSaveData = null;
            IsFileModified = false;
        }

        private void FileReload()
        {
            FileClose();

            // Only re-open if user didn't cancel close
            if (!IsFileOpen) {
                FileOpen(MostRecentPath);
            }
        }

        private void ApplicationExit()
        {
            Application.Current.MainWindow.Close();
        }

        private void ShowFileClosePrompt()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                "Do you want to save your changes?",
                "Save Changes?",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Yes,
                resultAction: FileClosePrompt_ResultAction));
        }

        private void FileClosePrompt_ResultAction(MessageBoxResult result)
        {
            // Does the user want to save before closing?
            switch (result) {
                case MessageBoxResult.Yes:
                    FileSave(MostRecentPath);
                    DoFileClose();
                    break;
                case MessageBoxResult.No:
                    DoFileClose();
                    break;
            }
        }

        #region Commands
        public ICommand FileOpenCommand
        {
            get {
                return new RelayCommand<string>(
                    (x) => OnFileDialogRequested(
                        new FileDialogEventArgs(FileDialogType.OpenDialog, resultAction: FileDialog_ResultAction)));
            }
        }

        public ICommand FileCloseCommand
        {
            get { return new RelayCommand(FileClose, () => IsFileOpen); }
        }

        public ICommand FileReloadCommand
        {
            get { return new RelayCommand(FileReload, () => IsFileOpen); }
        }

        public ICommand FileSaveCommand
        {
            get {
                return new RelayCommand<string>(
                    (x) => FileSave(MostRecentPath),
                    (x) => IsFileOpen);
            }
        }

        public ICommand FileSaveAsCommand
        {
            get {
                return new RelayCommand<string>(
                    (x) => OnFileDialogRequested(
                        new FileDialogEventArgs(FileDialogType.SaveDialog, resultAction: FileDialog_ResultAction)),
                    (x) => IsFileOpen);
            }
        }
        #endregion

        #region Event Handlers
        private void CarGenerators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Register property changed listener on each new element
            if (e.NewItems != null) {
                foreach (CarGenerator cg in e.NewItems) {
                    cg.PropertyChanged += CarGenerator_PropertyChanged; ;
                }
            }

            // Unregister property changed listener on each to-be-removed element
            if (e.OldItems != null) {
                foreach (CarGenerator cg in e.OldItems) {
                    cg.PropertyChanged -= CarGenerator_PropertyChanged;
                }
            }
        }

        private void CarGenerator_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Detect when a change was made to individual car generators
            IsFileModified = true;
        }

        public void FileDialog_ResultAction(bool? dialogResult, FileDialogEventArgs e)
        {
            if (dialogResult != true) {
                return;
            }

            switch (e.DialogType) {
                case FileDialogType.OpenDialog:
                    FileOpen(e.FileName);
                    break;
                case FileDialogType.SaveDialog:
                    FileSave(e.FileName);
                    break;
            }
        }
        #endregion

        #region Custom Events
        public event EventHandler<MessageBoxEventArgs> MessageBoxRequested;

        protected void OnMessageBoxRequested(MessageBoxEventArgs e)
        {
            MessageBoxRequested?.Invoke(this, e);
        }

        public event EventHandler<FileDialogEventArgs> FileDialogRequested;

        protected void OnFileDialogRequested(FileDialogEventArgs e)
        {
            FileDialogRequested?.Invoke(this, e);
        }
        #endregion
    }

    //public class MainViewModel : ObservableObject
    //{
    //    private SaveDataFile _saveDataFile;
    //    private string _saveDataFilePath;
    //    private ObservableCollection<CarGenerator> _carGens;
    //    private bool _showUnused;

    //    public MainViewModel()
    //    {
    //        CarGenerators = new ObservableCollection<CarGenerator>();
    //        CarGenerators.CollectionChanged += CarGenerators_CollectionChanged;
    //    }

    //    public SaveDataFile SaveDataFile
    //    {
    //        get {
    //            return _saveDataFile;
    //        }

    //        set {
    //            _saveDataFile = value;
    //            if (_saveDataFile == null) {
    //                CarGenerators = null;
    //            }
    //            else {
    //                foreach (CarGenerator cg in SaveDataFile.CarGenerators.CarGeneratorsArray) {
    //                    CarGenerators.Add(cg);
    //                }
    //            }

    //            OnPropertyChanged();
    //        }
    //    }

    //    private void CarGenerators_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //    {
    //        if (e.NewItems != null) {
    //            foreach (CarGenerator cg in e.NewItems) {
    //                cg.PropertyChanged += Cg_PropertyChanged;
    //            }
    //        }

    //        if (e.OldItems != null) {
    //            foreach (CarGenerator cg in e.OldItems) {
    //                cg.PropertyChanged -= Cg_PropertyChanged;
    //            }
    //        }
    //    }

    //    private void Cg_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //    {
    //        OnMessageBoxRequested(new MessageBoxEventArgs(null, "Something changed!"));
    //    }

    //    public string SaveDataFilePath
    //    {
    //        get { return _saveDataFilePath; }
    //        set { _saveDataFilePath = value; OnPropertyChanged(); }
    //    }

    //    public ObservableCollection<CarGenerator> CarGenerators
    //    {
    //        get { return _carGens; }
    //        set { _carGens = value; OnPropertyChanged(); }
    //    }

    //    public bool IsEditingFile
    //    {
    //        get { return _saveDataFile != null; }
    //    }

    //    public bool ShowUnused
    //    {
    //        get { return _showUnused; }
    //        set { _showUnused = value; OnPropertyChanged(); }
    //    }

    //    //#region Commands
    //    //public ICommand OpenFile
    //    //{
    //    //    get {
    //    //        return new RelayCommand(OpenFile_Execute);
    //    //    }
    //    //}

    //    //private void OpenFile_Execute()
    //    //{
    //    //    // TODO: this should be invoked by the view
    //    //    OpenFileDialog diag = new OpenFileDialog()
    //    //    {
    //    //        CheckFileExists = true,
    //    //        Multiselect = false,
    //    //        Filter = "GTA3 Save Data Files (*.b)|*.b|All Files (*.*)|*.*"
    //    //    };

    //    //    bool? fileSelected = diag.ShowDialog();
    //    //    if (fileSelected !=true) {
    //    //        return;
    //    //    }

    //    //    SaveDataFile = SaveDataFile.Load(diag.FileName);
    //    //    SaveDataFilePath = diag.FileName;
    //    //}

    //    //public ICommand CloseFile
    //    //{
    //    //    get {
    //    //        return new RelayCommand(CloseFile_Execute, CloseFile_CanExecute);
    //    //    }
    //    //}

    //    //private bool CloseFile_CanExecute()
    //    //{
    //    //    return IsEditingFile;
    //    //}

    //    //private void CloseFile_Execute()
    //    //{
    //    //    // TODO: prompt user to save
    //    //    SaveDataFile = null;
    //    //}

    //    //public ICommand SaveFile
    //    //{
    //    //    get {
    //    //        return new RelayCommand(SaveFile_Execute, SaveFile_CanExecute);
    //    //    }
    //    //}

    //    //private bool SaveFile_CanExecute()
    //    //{
    //    //    return IsEditingFile;
    //    //}

    //    //private void SaveFile_Execute()
    //    //{
    //    //    uint numCarGens = 0;
    //    //    foreach (CarGenerator cg in CarGenerators) {
    //    //        if (cg.Model != VehicleModel.None) {
    //    //            numCarGens++;
    //    //        }
    //    //    }

    //    //    SaveDataFile.CarGenerators.CarGeneratorsInfo.NumberOfCarGenerators = numCarGens;
    //    //    SaveDataFile.CarGenerators.CarGeneratorsArray = CarGenerators.ToArray();
    //    //    SaveDataFile.Store(SaveDataFilePath);
    //    //}

    //    //public ICommand SaveFileAs
    //    //{
    //    //    get {
    //    //        return new RelayCommand(SaveFileAs_Execute, SaveFileAs_CanExecute);
    //    //    }
    //    //}

    //    //private bool SaveFileAs_CanExecute()
    //    //{
    //    //    return IsEditingFile;
    //    //}

    //    //private void SaveFileAs_Execute()
    //    //{
    //    //    // TODO: this should be invoked by the view
    //    //    SaveFileDialog diag = new SaveFileDialog
    //    //    {
    //    //        Filter = "All Files (*.*)|*.*"
    //    //    };

    //    //    bool? fileSelected = diag.ShowDialog();
    //    //    if (fileSelected != true) {
    //    //        return;
    //    //    }

    //    //    SaveDataFilePath = diag.FileName;
    //    //    SaveFile.Execute(null);
    //    //}

    //    //public ICommand ExitApplication
    //    //{
    //    //    get {
    //    //        return new RelayCommand(ExitApplication_Execute);
    //    //    }
    //    //}

    //    //private void ExitApplication_Execute()
    //    //{
    //    //    Application.Current.MainWindow.Close();
    //    //}

    //    //public ICommand AboutApplication
    //    //{
    //    //    get {
    //    //        return new RelayCommand(AboutApplication_Execute);
    //    //    }
    //    //}

    //    //private void AboutApplication_Execute()
    //    //{
    //    //    OnMessageBoxRequested(new MessageBoxEventArgs(null, "AboutApplication"));
    //    //}
    //    //#endregion

    //    public event EventHandler<MessageBoxEventArgs> MessageBoxRequested;

    //    protected void OnMessageBoxRequested(MessageBoxEventArgs e)
    //    {
    //        MessageBoxRequested?.Invoke(this, e);
    //    }
    //}
}
