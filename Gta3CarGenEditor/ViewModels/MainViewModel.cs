using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
            WindowTitle = "GTA3 Car Generator Editor - " + path;
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
            WindowTitle = "GTA3 Car Generator Editor - " + path;
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

        private void UpdateNumberOfCarGenerators()
        {
            m_metadata.NumberOfCarGenerators = GetNumberOfCarGenerators();
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
            ShowImportSuccessDialog();

            // Refresh table
            ClearCarGeneratorsList();
            PopulateCarGeneratorsList();

            // Update status
            StatusText = "Imported car generators from " + path;
            IsFileModified = true;
        }

        private void ImportCarGeneratorsCSV(string path)
        {
            // Read car generators from CSV
            CarGenerator[] imported;
            try {
                imported = CarGeneratorCsvHelper.Read(path);
            }
            catch (IOException ex) {
                ShowErrorDialog(ex.Message);
                return;
            }
            catch (InvalidDataException ex) {
                ShowErrorDialog(ex.Message);
                return;
            }

            // Ensure number of car generators equals the game's expected amount
            int underBy = CarGeneratorsData.NumberOfCarGenerators - imported.Length;
            Array.Resize(ref imported, CarGeneratorsData.NumberOfCarGenerators);
            if (underBy > 0) {
                // Fill padded values with default car generator
                for (int i = 0; i < underBy; i++) {
                    imported[imported.Length - i - 1] = new CarGenerator();
                }
            }
            else if (underBy < 0) {
                ShowImportCountExceededDialog(Math.Abs(underBy));
            }

            // Replace car generators
            CurrentSaveData.CarGenerators.CarGeneratorsArray = imported;
            ShowImportSuccessDialog();

            // Refresh table
            ClearCarGeneratorsList();
            PopulateCarGeneratorsList();

            // Update metadata
            UpdateNumberOfCarGenerators();

            // Update status
            StatusText = "Imported car generators from " + path;
            IsFileModified = true;
        }

        private void ExportCarGeneratorsGTA3Save(string path)
        {
            // Load other file
            SaveDataFile saveData = LoadSaveData(path);
            if (saveData == null) {
                return;
            }

            // Replace car generators in other file and store file
            saveData.CarGenerators = CurrentSaveData.CarGenerators;
            if (!WriteSaveData(saveData, path)) {
                return;
            }

            ShowExportSuccessDialog();
            StatusText = "Exported car generators to " + path;
        }

        private void ExportCarGeneratorsCSV(string path)
        {
            CarGenerator[] data = CurrentSaveData.CarGenerators.CarGeneratorsArray;

            try {
                CarGeneratorCsvHelper.Write(data, path);
            }
            catch (IOException ex) {
                ShowErrorDialog(ex.Message);
                return;
            }
            catch (InvalidDataException ex) {
                ShowErrorDialog(ex.Message);
                return;
            }

            ShowExportSuccessDialog();
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

        private void ShowImportSuccessDialog()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                "Car generators imported successfully!",
                "Success",
                icon: MessageBoxImage.Information));
        }

        private void ShowExportSuccessDialog()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                "Car generators exported successfully!",
                "Success",
                icon: MessageBoxImage.Information));
        }

        private void ShowImportInfoDialog()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                "Select the file you want to import car generators from. The current set of car generators will be overwritten.",
                "Import Car Generators",
                icon: MessageBoxImage.Information));
        }

        private void ShowExportInfoDialog()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                "Select the file you want to export the current set of car generators to. The car generators in that file will be overwritten.",
                "Export Car Generators",
                icon: MessageBoxImage.Information));
        }

        private void ShowImportCountExceededDialog(int overBy)
        {
            string msg = string.Format("The number of car generators imported exceeds the maximum amount by {0}. The extra car generators will be omitted.", overBy);
            OnMessageBoxRequested(new MessageBoxEventArgs(
                msg,
                "Limit Exceeded",
                icon: MessageBoxImage.Warning));
        }

        private void ShowAboutDialog()
        {
            string msg = string.Format("GTA3 Car Generator Editor\n" +
                "by W. Hampson (a.k.a. thehambone)\n" +
                "Version: {0}\n\n" +
                "This tool allows you to edit the parked car generators in a GTA3 savegame. You can control the car that spawns, it's location, color, and more! You can also transfer car generators between saves on any platform.\n\n" +
                "Special thanks to GTAKid667 for providing feedback and support during development.\n\n" +
                "Copyright (C) 2018 W. Hampson. All rights reserved.",
                GetVersionString());

            OnMessageBoxRequested(new MessageBoxEventArgs(
                msg,
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information));
        }

        private string GetVersionString()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            if (asm == null) {
                return "(not available)";
            }

            FileVersionInfo vInfo = FileVersionInfo.GetVersionInfo(asm.Location);
            return string.Format("{0} (build {1})",
                vInfo.ProductVersion, vInfo.FilePrivatePart);
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
                    (x) => ExecuteImportFromSaveDataCommand(),
                    (x) => IsFileOpen);
            }
        }

        private void ExecuteImportFromSaveDataCommand()
        {
            ShowImportInfoDialog();
            ShowGTA3SaveDataOpenDialog(ImportCarGeneratorsGTA3SaveDialog_ResultAction);
        }

        public ICommand ImportFromCSVCommand
        {
            get {
                return new RelayCommand<Action<bool?, FileDialogEventArgs>>(
                    (x) => ExecuteImportFromCSVCommand(),
                    (x) => IsFileOpen);
            }
        }

        private void ExecuteImportFromCSVCommand()
        {
            ShowImportInfoDialog();
            ShowCSVOpenDialog(ImportExportCarGeneratorsCSVDialog_ResultAction);
        }

        public ICommand ExportToSaveDataCommand
        {
            get {
                return new RelayCommand(ExecuteExportToSaveDataCommand, () => IsFileOpen);
            }
        }

        private void ExecuteExportToSaveDataCommand()
        {
            ShowExportInfoDialog();
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

        public ICommand CheckForUpdatesCommand
        {
            get {
                return new RelayCommand(
                    () => Process.Start("https://github.com/whampson/gta3-cargen-editor/releases"));
            }
        }

        public ICommand ShowAboutDialogCommand
        {
            get { return new RelayCommand(ShowAboutDialog); }
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
                UpdateNumberOfCarGenerators();
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
