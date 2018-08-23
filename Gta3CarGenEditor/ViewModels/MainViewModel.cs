using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WHampson.Gta3CarGenEditor.Events;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Models;
using WHampson.Gta3CarGenEditor.Properties;

namespace WHampson.Gta3CarGenEditor.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        #region Private Fields
        private SaveDataFile m_currentSaveData;
        private ObservableCollection<CarGenerator> m_carGenerators;
        private CarGeneratorsInfo m_metadata;
        private string m_mostRecentPath;
        private bool m_isShowingUnusedFields;
        private bool m_isFileModified;
        private string m_windowTitle;
        private string m_statusText;
        #endregion

        #region Constructors
        public MainViewModel()
        {
            m_carGenerators = new ObservableCollection<CarGenerator>();
            m_carGenerators.CollectionChanged += CarGenerators_CollectionChanged;
            m_windowTitle = "GTA3 Car Generator Editor";
            m_statusText = "No file opened.";
        }
        #endregion

        #region Public Properties
        public SaveDataFile CurrentSaveData
        {
            get { return m_currentSaveData; }
            set {
                m_currentSaveData = value;
                if (m_currentSaveData != null) {
                    m_metadata = m_currentSaveData.CarGenerators.CarGeneratorsInfo;
                    m_metadata.PropertyChanged += CarGenerators_PropertyChanged;
                    PopulateCarGeneratorsList();
                }
                else {
                    if (m_metadata != null) {
                        m_metadata.PropertyChanged -= CarGenerators_PropertyChanged;
                        m_metadata = null;
                    }
                    ClearCarGeneratorsList();
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFileOpen));
            }
        }

        public ObservableCollection<CarGenerator> CarGeneratorsList
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

        public string WindowTitle
        {
            get { return m_windowTitle; }
            set { m_windowTitle = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get { return m_statusText; }
            set { m_statusText = value; OnPropertyChanged(); }
        }
        #endregion

        #region Private Functions
        private void FileOpen(string path)
        {
            if (IsFileOpen) {
                FileClose();
            }

            if (IsFileOpen) {
                return;
            }

            SaveDataFile saveData = LoadSaveData(path);
            if (saveData == null) {
                return;
            }

            CurrentSaveData = saveData;
            MostRecentPath = path;
            WindowTitle = "GTA3 Car Generators Editor - " + path;
            StatusText = "File opened for edit.";
        }

        private SaveDataFile LoadSaveData(string path)
        {
            SaveDataFile saveData = null;
            try {
                saveData = SaveDataFile.Load(path);
            }
            catch (IOException ex) {
                ShowErrorDialog(ex.Message);
            }
            catch (InvalidDataException ex) {
                ShowErrorDialog(ex.Message);
            }

            return saveData;
        }

        private void FileSave(string path)
        {
            bool result = WriteSaveData(CurrentSaveData, path);
            if (!result) {
                return;
            }

            MostRecentPath = path;
            WindowTitle = "GTA3 Car Generators Editor - " + path;
            StatusText = "File saved successfully.";
            IsFileModified = false;
        }

        private bool WriteSaveData(SaveDataFile saveData, string path)
        {
            bool result = false;

            try {
                saveData.Store(path);
                result = true;
            }
            catch (IOException ex) {
                ShowErrorDialog(ex.Message);
            }
            catch (InvalidDataException ex) {
                ShowErrorDialog(ex.Message);
            }

            return result;
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
            CurrentSaveData = null;
            IsFileModified = false;
            WindowTitle = "GTA3 Car Generators Editor";
            StatusText = "No file opened.";
        }

        private void ZeroOutTimers()
        {
            foreach (CarGenerator cg in CarGeneratorsList) {
                cg.Timer = 0;
            }
        }

        private void PopulateCarGeneratorsList()
        {
            foreach (CarGenerator cg in CurrentSaveData.CarGenerators.CarGeneratorsArray) {
                CarGeneratorsList.Add(cg);
            }
        }

        private void ClearCarGeneratorsList()
        {
            CarGeneratorsList.Clear();
        }

        private uint GetNumberOfCarGenerators()
        {
            uint count = 0;
            foreach (CarGenerator cg in CarGeneratorsList) {
                if (cg.Model != VehicleModel.None) {
                    count++;
                }
            }

            return count;
        }

        private void ImportCarGeneratorsGTA3Save(string path)
        {
            // Load other file
            SaveDataFile saveData = LoadSaveData(path);
            if (saveData == null) {
                return;
            }

            // Replace car generators
            CurrentSaveData.CarGenerators = saveData.CarGenerators;

            // Show success message
            OnMessageBoxRequested(new MessageBoxEventArgs(
                "Car generators imported successfully!",
                "Success",
                icon: MessageBoxImage.Information));

            // Refresh table
            ClearCarGeneratorsList();
            PopulateCarGeneratorsList();

            // Update status
            StatusText = "Imported car generators from " + path;
            IsFileModified = true;
        }

        private void ImportCarGeneratorsCSV(string path)
        {
            // TODO: implement
            OnMessageBoxRequested(new MessageBoxEventArgs("Import cargens CSV: " + path));
            StatusText = "Imported car generators from " + path;
        }

        private void ExportCarGeneratorsGTA3Save(string path)
        {
            // Load other file
            SaveDataFile saveData = LoadSaveData(path);
            if (saveData == null) {
                return;
            }

            // Replace car generators
            saveData.CarGenerators = CurrentSaveData.CarGenerators;

            // Store other file
            if (!WriteSaveData(saveData, path)) {
                return;
            }

            // Show success message
            OnMessageBoxRequested(new MessageBoxEventArgs(
                "Car generators exported successfully!",
                "Success",
                icon: MessageBoxImage.Information));

            // Update status
            StatusText = "Exported car generators to " + path;
        }

        private void ExportCarGeneratorsCSV(string path)
        {
            // TODO: implement
            OnMessageBoxRequested(new MessageBoxEventArgs("Export cargens CSV: " + path));
            StatusText = "Exported car generators to " + path;
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

        private void ShowGTA3SaveDataOpenDialog(Action<bool?, FileDialogEventArgs> resultAction)
        {
            OnFileDialogRequested(new FileDialogEventArgs(
                FileDialogType.OpenDialog,
                filter: "GTA3 Save Data (*.b)|*.b|All Files (*.*)|*.*",
                title: "Open...",
                resultAction: resultAction));
        }

        private void ShowGTA3SaveDataSaveDialog(Action<bool?, FileDialogEventArgs> resultAction)
        {
            OnFileDialogRequested(new FileDialogEventArgs(
                FileDialogType.SaveDialog,
                fileName: Path.GetFileName(MostRecentPath),
                filter: "GTA3 Save Data (*.b)|*.b|All Files (*.*)|*.*",
                title: "Save As...",
                resultAction: resultAction));
        }

        private void ShowCSVOpenDialog(Action<bool?, FileDialogEventArgs> resultAction)
        {
            OnFileDialogRequested(new FileDialogEventArgs(
                FileDialogType.OpenDialog,
                filter: "CSV (comma-delimited) (*.csv)|*.csv|All Files (*.*)|*.*",
                resultAction: resultAction));
        }

        private void ShowCSVSaveDialog(Action<bool?, FileDialogEventArgs> resultAction)
        {
            OnFileDialogRequested(new FileDialogEventArgs(
                FileDialogType.SaveDialog,
                filter: "CSV (comma-delimited) (*.csv)|*.csv|All Files (*.*)|*.*",
                resultAction: resultAction));
        }

        private void ShowErrorDialog(string message)
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                message,
                "Error",
                icon: MessageBoxImage.Error));
        }

        #endregion

        #region Commands
        public ICommand FileOpenCommand
        {
            get {
                return new RelayCommand<Action<bool?, FileDialogEventArgs>>(
                    (x) => ShowGTA3SaveDataOpenDialog(OpenSaveGTA3SaveDataDialog_ResultAction));
            }
        }

        public ICommand FileCloseCommand
        {
            get { return new RelayCommand(FileClose, () => IsFileOpen); }
        }

        public ICommand FileSaveCommand
        {
            get {
                return new RelayCommand<Action<bool?, FileDialogEventArgs>>(
                    (x) => FileSave(MostRecentPath),
                    (x) => IsFileOpen);
            }
        }

        public ICommand FileSaveAsCommand
        {
            get {
                return new RelayCommand<Action<bool?, FileDialogEventArgs>>(
                    (x) => ShowGTA3SaveDataSaveDialog(OpenSaveGTA3SaveDataDialog_ResultAction),
                    (x) => IsFileOpen);
            }
        }

        public ICommand ImportFromSaveDataCommand
        {
            get {
                return new RelayCommand<Action<bool?, FileDialogEventArgs>>(
                    (x) => ExecuteImportToSaveDataCommand(),
                    (x) => IsFileOpen);
            }
        }

        private void ExecuteImportToSaveDataCommand()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                "Select the file you want to import car generators from. The current set of car generators will be overwritten.", 
                "Import Car Generators",
                icon: MessageBoxImage.Information));

            ShowGTA3SaveDataOpenDialog(ImportCarGeneratorsGTA3SaveDialog_ResultAction);
        }

        public ICommand ImportFromCSVCommand
        {
            get {
                return new RelayCommand<Action<bool?, FileDialogEventArgs>>(
                    (x) => ShowCSVOpenDialog(ImportExportCarGeneratorsCSVDialog_ResultAction),
                    (x) => IsFileOpen);
            }
        }

        public ICommand ExportToSaveDataCommand
        {
            get {
                return new RelayCommand(ExecuteExportToSaveDataCommand, () => IsFileOpen);
            }
        }

        private void ExecuteExportToSaveDataCommand()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                "Select the file you want to export the current set of car generators to. The car generators in that file will be overwritten.",
                "Export Car Generators",
                icon: MessageBoxImage.Information));

            ShowGTA3SaveDataOpenDialog(ExportCarGeneratorsGTA3SaveDialog_ResultAction);
        }

        public ICommand ExportToCSVCommand
        {
            get {
                return new RelayCommand<Action<bool?, FileDialogEventArgs>>(
                    (x) => ShowCSVSaveDialog(ImportExportCarGeneratorsCSVDialog_ResultAction),
                    (x) => IsFileOpen);
            }
        }

        public ICommand ApplicationExitCommand
        {
            get { return new RelayCommand(ApplicationExit); }
        }

        public ICommand EditMetadataCommand
        {
            get {
                return new RelayCommand(
                    () => OnEditMetadataDialogRequested(
                        new MetadataDialogEventArgs(m_metadata)),
                    () => IsFileOpen);
            }
        }

        public ICommand ZeroOutTimersCommand
        {
            get { return new RelayCommand(ZeroOutTimers, () => IsFileOpen); }
        }

        public ICommand ShowUnusedFieldsCommand
        {
            get { return new RelayCommand(() => IsShowingUnusedFields = !IsShowingUnusedFields); }
        }

        public ICommand ShowAboutDialogCommand
        {
            // TODO: finish text
            get {
                return new RelayCommand(
                    () => OnMessageBoxRequested(
                        new MessageBoxEventArgs(
                            "About this app...",
                            "About",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information)));
            }
        }
        #endregion

        #region Event Handlers
        private void CarGenerators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Register property changed listener on each new element
            if (e.NewItems != null) {
                foreach (CarGenerator cg in e.NewItems) {
                    cg.PropertyChanged += CarGenerators_PropertyChanged;
                }
            }

            // Unregister property changed listener on each to-be-removed element
            if (e.OldItems != null) {
                foreach (CarGenerator cg in e.OldItems) {
                    cg.PropertyChanged -= CarGenerators_PropertyChanged;
                }
            }
        }

        private void CarGenerators_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Detect when a change was made to individual car generators
            IsFileModified = true;

            // Update car generator count in metadata
            if (e.PropertyName == nameof(CarGenerator.Model)) {
                m_metadata.NumberOfCarGenerators = GetNumberOfCarGenerators();
            }
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

        public void OpenSaveGTA3SaveDataDialog_ResultAction(bool? dialogResult, FileDialogEventArgs e)
        {
            // Ignore if anything other than "OK" pressed
            if (dialogResult != true) {
                return;
            }

            // Open or Save based on dialog type
            switch (e.DialogType) {
                case FileDialogType.OpenDialog:
                    FileOpen(e.FileName);
                    break;
                case FileDialogType.SaveDialog:
                    FileSave(e.FileName);
                    break;
            }
        }

        public void ImportCarGeneratorsGTA3SaveDialog_ResultAction(bool? dialogResult, FileDialogEventArgs e)
        {
            // Ignore if anything other than "OK" pressed
            if (dialogResult != true) {
                return;
            }

            ImportCarGeneratorsGTA3Save(e.FileName);
        }

        public void ExportCarGeneratorsGTA3SaveDialog_ResultAction(bool? dialogResult, FileDialogEventArgs e)
        {
            // Ignore if anything other than "OK" pressed
            if (dialogResult != true) {
                return;
            }

            ExportCarGeneratorsGTA3Save(e.FileName);
        }

        public void ImportExportCarGeneratorsCSVDialog_ResultAction(bool? dialogResult, FileDialogEventArgs e)
        {
            // Ignore if anything other than "OK" pressed
            if (dialogResult != true) {
                return;
            }

            // Open or Save based on dialog type
            switch (e.DialogType) {
                case FileDialogType.OpenDialog:
                    ImportCarGeneratorsCSV(e.FileName);
                    break;
                case FileDialogType.SaveDialog:
                    ExportCarGeneratorsCSV(e.FileName);
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

        public event EventHandler<MetadataDialogEventArgs> EditMetadataDialogRequested;
        protected void OnEditMetadataDialogRequested(MetadataDialogEventArgs e)
        {
            EditMetadataDialogRequested?.Invoke(this, e);
        }
        #endregion
    }
}
