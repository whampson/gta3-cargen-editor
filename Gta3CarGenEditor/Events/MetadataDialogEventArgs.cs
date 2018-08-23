using System;
using WHampson.Gta3CarGenEditor.Models;

namespace WHampson.Gta3CarGenEditor.Events
{
    /// <summary>
    /// Parameters for opening a MetadataWindow.
    /// </summary>
    public class MetadataDialogEventArgs : EventArgs
    {
        public MetadataDialogEventArgs(CarGeneratorsInfo metadata)
        {
            Metadata = metadata;
        }

        public CarGeneratorsInfo Metadata
        {
            get;
        }
    }
}
