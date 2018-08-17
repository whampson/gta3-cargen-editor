namespace WHampson.Gta3CarGenEditor.Models
{
    /// <summary>
    /// Represents a data container for a <see cref="SaveDataFile"/>.
    /// </summary>
    public class DataBlock
    {
        /// <summary>
        /// Creates a new <see cref="DataBlock"/> object with no
        /// <see cref="Tag"/>, zero bytes of <see cref="Data"/>, and
        /// <see cref="StoreBlockSize"/> set to true.
        /// </summary>
        public DataBlock()
        {
            Data = new byte[0];
            Tag = null;
            StoreBlockSize = true;
        }

        /// <summary>
        /// Gets or sets the data stored in this <see cref="DataBlock"/>.
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the size of the
        /// data should be read or written along with the data itself.
        /// </summary>
        public bool StoreBlockSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the block identifier.
        /// </summary>
        public string Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="DataBlock"/>
        /// has a <see cref="Tag"/>.
        /// </summary>
        public bool HasTag
        {
            get { return Tag != null; }
        }
    }
}
