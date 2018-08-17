using Microsoft.Win32;
using System;
using System.Windows;
using WHampson.Gta3CarGenEditor.Properties;

namespace WHampson.Gta3CarGenEditor.Helpers
{
    /// <summary>
    /// Parameters for opening a <see cref="FileDialog"/> from an event.
    /// </summary>
    public class FileDialogEventArgs : EventArgs
    {
        public FileDialogEventArgs(FileDialogType dialogType,
            string title = null,
            string filter = null,
            Action<bool?, FileDialogEventArgs> resultAction = null)
        {
            Title = title;
            Filter = filter;
            ResultAction = resultAction;
        }

        public FileDialogType DialogType
        {
            get;
        }

        public string Title
        {
            get;
        }

        public string Filter
        {
            get;
        }

        public string FileName
        {
            get;
            set;
        }

        public Action<bool?, FileDialogEventArgs> ResultAction
        {
            get;
        }

        public void ShowDialog()
        {
            ShowDialog(null);
        }

        public void ShowDialog(Window owner)
        {
            FileDialog dialog;
            switch (DialogType) {
                case FileDialogType.OpenDialog:
                    dialog = new OpenFileDialog();
                    break;
                case FileDialogType.SaveDialog:
                    dialog = new SaveFileDialog();
                    break;
                default:
                    throw new InvalidOperationException(Resources.OopsMessage);
            }

            dialog.Title = Title;
            dialog.Filter = Filter;

            bool? result;
            if (owner == null) {
                result = dialog.ShowDialog();
            }
            else {
                result = dialog.ShowDialog(owner);
            }

            if (result == true) {
                FileName = dialog.FileName;
            }

            ResultAction?.Invoke(result, this);
        }

        public enum FileDialogType
        {
            OpenDialog,
            SaveDialog
        }
    }
}
