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
using WHampson.Gta3CarGenEditor.Resources;

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
            m_windowTitle = Strings.AppName;
            m_statusText = Strings.StatusMessageNoFileOpened;
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
                    OnResetRowOrderRequested();
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
            WindowTitle = string.Format("{0} - {1}", Strings.AppName, path);
            StatusText = Strings.StatusMessageFileOpened;
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
                StatusText = Strings.StatusMessageSaveUnsuccessful;
                return;
            }

            MostRecentPath = path;
            WindowTitle = string.Format("{0} - {1}", Strings.AppName, path);
            StatusText = Strings.StatusMessageSaveSuccessful;
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
            WindowTitle = Strings.AppName;
            StatusText = Strings.StatusMessageNoFileOpened;
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
                StatusText = Strings.StatusMessageImportUnsuccessful;
                return;
            }

            // Replace car generators
            CurrentSaveData.CarGenerators = saveData.CarGenerators;
            ShowImportSuccessDialog();

            // Refresh table
            ClearCarGeneratorsList();
            PopulateCarGeneratorsList();
            OnResetRowOrderRequested();

            // Update status
            StatusText = string.Format(Strings.StatusMessageImportSuccessful, path);
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
                StatusText = Strings.StatusMessageImportUnsuccessful;
                return;
            }
            catch (InvalidDataException ex) {
                ShowErrorDialog(ex.Message);
                StatusText = Strings.StatusMessageImportUnsuccessful;
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
            OnResetRowOrderRequested();

            // Update metadata
            UpdateNumberOfCarGenerators();

            // Update status
            StatusText = string.Format(Strings.StatusMessageImportSuccessful, path);
            IsFileModified = true;
        }

        private void ExportCarGeneratorsGTA3Save(string path)
        {
            // Load other file
            SaveDataFile saveData = LoadSaveData(path);
            if (saveData == null) {
                StatusText = Strings.StatusMessageExportUnsuccessful;
                return;
            }

            // Replace car generators in other file and store file
            saveData.CarGenerators = CurrentSaveData.CarGenerators;
            if (!WriteSaveData(saveData, path)) {
                StatusText = Strings.StatusMessageExportUnsuccessful;
                return;
            }

            ShowExportSuccessDialog();
            StatusText = string.Format(Strings.StatusMessageExportSuccessful, path);
        }

        private void ExportCarGeneratorsCSV(string path)
        {
            CarGenerator[] data = CurrentSaveData.CarGenerators.CarGeneratorsArray;

            try {
                CarGeneratorCsvHelper.Write(data, path);
            }
            catch (IOException ex) {
                ShowErrorDialog(ex.Message);
                StatusText = Strings.StatusMessageExportUnsuccessful;
                return;
            }
            catch (InvalidDataException ex) {
                ShowErrorDialog(ex.Message);
                StatusText = Strings.StatusMessageExportUnsuccessful;
                return;
            }

            ShowExportSuccessDialog();
            StatusText = string.Format(Strings.StatusMessageExportSuccessful, path);
        }

        private void ApplicationExit()
        {
            Application.Current.MainWindow.Close();
        }

        private void ShowFileClosePrompt()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                Strings.DialogMessageSaveChangesPrompt,
                Strings.DialogTitleSaveChangesPrompt,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Yes,
                resultAction: FileClosePrompt_ResultAction));
        }

        private void ShowGTA3SaveDataOpenDialog(Action<bool?, FileDialogEventArgs> resultAction)
        {
            OnFileDialogRequested(new FileDialogEventArgs(
                FileDialogType.OpenDialog,
                filter: Strings.FilterGta3SaveData,
                title: Strings.DialogTitleOpenFile,
                resultAction: resultAction));
        }

        private void ShowGTA3SaveDataSaveDialog(Action<bool?, FileDialogEventArgs> resultAction)
        {
            OnFileDialogRequested(new FileDialogEventArgs(
                FileDialogType.SaveDialog,
                fileName: Path.GetFileName(MostRecentPath),
                filter: Strings.FilterGta3SaveData,
                title: Strings.DialogTitleSaveFileAs,
                resultAction: resultAction));
        }

        private void ShowCSVOpenDialog(Action<bool?, FileDialogEventArgs> resultAction)
        {
            OnFileDialogRequested(new FileDialogEventArgs(
                FileDialogType.OpenDialog,
                filter: Strings.FilterCsv,
                title: Strings.DialogTitleOpenFile,
                resultAction: resultAction));
        }

        private void ShowCSVSaveDialog(Action<bool?, FileDialogEventArgs> resultAction)
        {
            OnFileDialogRequested(new FileDialogEventArgs(
                FileDialogType.SaveDialog,
                filter: Strings.FilterCsv,
                title: Strings.DialogTitleSaveFileAs,
                resultAction: resultAction));
        }

        private void ShowErrorDialog(string message)
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                message,
                Strings.DialogTitleError,
                icon: MessageBoxImage.Error));
        }

        private void ShowImportSuccessDialog()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                Strings.DialogMessageImportSuccessful,
                Strings.DialogTitleSuccess,
                icon: MessageBoxImage.Information));
        }

        private void ShowExportSuccessDialog()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                Strings.DialogMessageExportSuccessful,
                Strings.DialogTitleSuccess,
                icon: MessageBoxImage.Information));
        }

        private void ShowImportInfoDialog()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                Strings.DialogMessageImportInfo,
                Strings.DialogTitleImportInfo,
                icon: MessageBoxImage.Information));
        }

        private void ShowExportInfoDialog()
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                Strings.DialogMessageExportInfo,
                Strings.DialogTitleExportInfo,
                icon: MessageBoxImage.Information));
        }

        private void ShowImportCountExceededDialog(int overBy)
        {
            OnMessageBoxRequested(new MessageBoxEventArgs(
                string.Format(Strings.DialogMessageImportLimitExceeded, overBy),
                Strings.DialogTitleImportLimitExceeded,
                icon: MessageBoxImage.Warning));
        }

        private void ShowAboutDialog()
        {
            string msg = string.Format("{0}\n" +
                "{1}\n" +
                "{2}\n\n" +
                "{3}\n\n" +
                "{4}\n\n\n" +
                "{5}",
                Strings.AppName,
                Strings.AppAuthor,
                string.Format(Strings.AppVersion, GetVersionString()),
                Strings.AppDescriptionLong,
                Strings.AppSpecialThanks,
                Strings.AppCopyright);

            OnMessageBoxRequested(new MessageBoxEventArgs(
                msg,
                Strings.DialogTitleAbout,
                MessageBoxButton.OK,
                MessageBoxImage.Information));
        }

        private string GetVersionString()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            if (asm == null) {
                return Strings.AppVersionError;
            }

            FileVersionInfo vInfo = FileVersionInfo.GetVersionInfo(asm.Location);
            return string.Format(Strings.AppVersionFormat,
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

        public ICommand ResetRowOrderCommand
        {
            get { return new RelayCommand(OnResetRowOrderRequested); }
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

        public event EventHandler<EventArgs> ResetRowOrderRequested;
        protected void OnResetRowOrderRequested()
        {
            ResetRowOrderRequested?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}
