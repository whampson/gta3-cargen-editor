using System;

namespace WHampson.Gta3CarGenEditor.Helpers
{
    /// <summary>
    /// Parameters for closing a dialog window from an event.
    /// </summary>
    public class DialogCloseEventArgs : EventArgs
    {
        public DialogCloseEventArgs(bool? dialogResult = null)
        {
            DialogResult = dialogResult;
        }

        public bool? DialogResult
        {
            get;
        }
    }
}
