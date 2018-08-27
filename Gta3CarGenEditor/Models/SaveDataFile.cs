using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WHampson.Gta3CarGenEditor.Extensions;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Resources;

namespace WHampson.Gta3CarGenEditor.Models
{
    /// <summary>
    /// Represents a Grand Theft Auto III save data file.
    /// </summary>
    public abstract class SaveDataFile : SerializableObject
    {
        private const string ScriptsTag = "SCR\0";
        private const string RestartsTag = "RST\0";
        private const string RadarTag = "RDR\0";
        private const string ZonesTag = "ZNS\0";
        private const string GangsTag = "GNG\0";
        private const string CarGeneratorsTag = "CGN\0";
        private const string AudioScriptObjectsTag = "AUD\0";
        private const string PedTypesTag = "PTP\0";

        protected DataBlock m_simpleVars;
        protected DataBlock m_scripts;
        protected DataBlock m_playerPeds;
        protected DataBlock m_garages;
        protected DataBlock m_vehicles;
        protected DataBlock m_objects;
        protected DataBlock m_pathFind;
        protected DataBlock m_cranes;
        protected DataBlock m_pickups;
        protected DataBlock m_phoneInfo;
        protected DataBlock m_restarts;
        protected DataBlock m_radar;
        protected DataBlock m_zones;
        protected DataBlock m_gangs;
        protected DataBlock m_carGenerators;
        protected DataBlock m_particles;
        protected DataBlock m_audioScriptObjects;
        protected DataBlock m_playerInfo;
        protected DataBlock m_stats;
        protected DataBlock m_streaming;
        protected DataBlock m_pedTypes;
        protected DataBlock[] m_padding;

        protected SaveDataFile(GamePlatform fileType)
        {
            FileType = fileType;
            CarGenerators = new CarGeneratorsData();

            m_simpleVars            = new DataBlock() { StoreBlockSize = false };
            m_scripts               = new DataBlock() { Tag = ScriptsTag };
            m_playerPeds            = new DataBlock();
            m_garages               = new DataBlock();
            m_vehicles              = new DataBlock();
            m_objects               = new DataBlock();
            m_pathFind              = new DataBlock();
            m_cranes                = new DataBlock();
            m_pickups               = new DataBlock();
            m_phoneInfo             = new DataBlock();
            m_restarts              = new DataBlock() { Tag = RestartsTag };
            m_radar                 = new DataBlock() { Tag = RadarTag };
            m_zones                 = new DataBlock() { Tag = ZonesTag };
            m_gangs                 = new DataBlock() { Tag = GangsTag };
            m_carGenerators         = new DataBlock() { Tag = CarGeneratorsTag };
            m_particles             = new DataBlock();
            m_audioScriptObjects    = new DataBlock() { Tag = AudioScriptObjectsTag };
            m_playerInfo            = new DataBlock();
            m_stats                 = new DataBlock();
            m_streaming             = new DataBlock();
            m_pedTypes              = new DataBlock() { Tag = PedTypesTag };
            m_padding               = new DataBlock[0];
        }

        /// <summary>
        /// Gets the file format type based on the <see cref="GamePlatform"/>
        /// that created this save data.
        /// </summary>
        public GamePlatform FileType
        {
            get;
        }

        /// <summary>
        /// Gets or sets the car generator data.
        /// </summary>
        public CarGeneratorsData CarGenerators
        {
            get;
            set;
        }

        /// <summary>
        /// Writes this saved game data to a file.
        /// </summary>
        /// <param name="path">The file to write.</param>
        public void Store(string path)
        {
            byte[] data = Serialize(this);
            File.WriteAllBytes(path, data);
        }

        /// <summary>
        /// Computes the checksum that goes in the footer of the file.
        /// </summary>
        /// <param name="stream">The stream containing the serialized save data.</param>
        /// <returns>The serialized data checksum.</returns>
        protected int GetChecksum(Stream stream)
        {
            using (MemoryStream m = new MemoryStream()) {
                stream.Position = 0;
                stream.CopyTo(m);
                return m.ToArray().Sum(x => x);
            }
        }

        /// <summary>
        /// Sets the Data field of a <see cref="DataBlock"/> by
        /// reading data at the current position in a stream in
        /// accordance with the <see cref="DataBlock"/> parameters.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="block">The block to populate.</param>
        /// <returns>The number of bytes read.</returns>
        protected int ReadDataBlock(Stream stream, DataBlock block)
        {
            // Format:
            //     (optional) BlockSize
            //     (optional) Tag
            //     (optional) BlockSize - sizeof(Tag)
            //     (required) Data

            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                int blockSize;
                int nestedBlockSize;
                int paddingSize;

                // Get block size
                if (block.StoreBlockSize) {
                    // Read block size from stream
                    blockSize = r.ReadInt32();
                }
                else {
                    // Use pre-allocated array size
                    blockSize = block.Data.Length;
                }

                if (block.HasTag) {
                    // Read tag
                    string tag = Encoding.ASCII.GetString(r.ReadBytes(block.Tag.Length));
                    if (tag != block.Tag) {
                        string msg = string.Format(Strings.InvalidBlockTagMessage,
                            tag.StripNull(), block.Tag.StripNull());
                        throw new InvalidDataException(msg);
                    }

                    // Read nested block size
                    if (block.StoreBlockSize) {
                        nestedBlockSize = r.ReadInt32();
                        if (nestedBlockSize != blockSize - tag.Length - 4) {
                            throw new InvalidDataException(Strings.IncorrectBlockSizeReadMessage);
                        }
                        blockSize = nestedBlockSize;
                    }
                }

                // Compute padding
                paddingSize = Align32(blockSize) - blockSize;

                // Read data
                block.Data = r.ReadBytes(blockSize);

                // Read padding
                r.ReadBytes(paddingSize);
            }

            return (int) (stream.Position - start);
        }

        /// <summary>
        /// Reads a big data block from the stream at the current position.
        /// A big data block is defined as a tagless <see cref="DataBlock"/> containing
        /// at least one nested <see cref="DataBlock"/> inside of it.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="blocks">The nested blocks to populate.</param>
        /// <returns>The number of bytes read.</returns>
        protected int ReadBigDataBlock(Stream stream, params DataBlock[] blocks)
        {
            long start = stream.Position;
            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                int totalSize;
                int bytesRead;

                totalSize = r.ReadInt32();

                bytesRead = 0;
                foreach (DataBlock block in blocks) {
                    bytesRead += ReadDataBlock(stream, block);
                }

                if (bytesRead != totalSize) {
                    throw new InvalidDataException(Strings.IncorrectNumberOfBytesDecodedMessage);
                }
            }

            return (int) (stream.Position - start);
        }

        protected int ReadPadding(Stream stream)
        {
            int count = CountRemainingBlocks(stream);
            int bytesRead = 0;

            m_padding = new DataBlock[count];
            for (int i = 0; i < count; i++) {
                m_padding[i] = new DataBlock();
                bytesRead += ReadDataBlock(stream, m_padding[i]);
            }

            return bytesRead;
        }

        /// <summary>
        /// Writes the Data field of a <see cref="DataBlock"/> to
        /// a stream at the current position in accordance with the
        /// <see cref="DataBlock"/> parameters.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="block">The block to write.</param>
        /// <returns>The number of bytes written.</returns>
        protected int WriteDataBlock(Stream stream, DataBlock block)
        {
            // Format:
            //     (optional) BlockSize
            //     (optional) Tag
            //     (optional) BlockSize - sizeof(Tag)
            //     (required) Data

            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                int blockSize;
                int nestedBlockSize;
                int paddingSize;

                // Write block size
                blockSize = GetBlockSize(block);
                if (block.StoreBlockSize) {
                    w.Write(blockSize);     // Subtract 4 because stored size does not include itself
                }

                if (block.HasTag) {
                    // Write tag
                    w.Write(Encoding.ASCII.GetBytes(block.Tag));

                    // Write nested block size
                    if (block.StoreBlockSize) {
                        nestedBlockSize = blockSize - block.Tag.Length - 4;
                        w.Write(nestedBlockSize);
                        blockSize = nestedBlockSize;
                    }
                }

                Debug.Assert(block.Data.Length == blockSize);

                // Compute padding
                paddingSize = Align32(blockSize) - blockSize;

                // Write data
                w.Write(block.Data);

                // Write padding
                w.Write(new byte[paddingSize]);
            }

            return (int) (stream.Position - start);
        }

        /// <summary>
        /// Writes a big data block to the stream at the current position.
        /// A big data block is defined as a tagless <see cref="DataBlock"/> containing
        /// at least one nested <see cref="DataBlock"/> inside of it.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="blocks">The nested blocks to write.</param>
        /// <returns>The number of bytes written.</returns>
        protected int WriteBigDataBlock(Stream stream, params DataBlock[] blocks)
        {
            long start = stream.Position;
            using (BinaryWriter w = new BinaryWriter(stream, Encoding.Default, true)) {
                int totalSize;
                int bytesWritten;

                totalSize = 0;
                foreach (DataBlock block in blocks) {
                    totalSize += Align32(GetBlockSize(block));
                    if (block.StoreBlockSize) {
                        totalSize += 4;
                    }
                }

                w.Write(totalSize);
                bytesWritten = 4;

                foreach (DataBlock block in blocks) {
                    bytesWritten += WriteDataBlock(stream, block);
                }

                if (bytesWritten != totalSize + 4) {
                    throw new InvalidDataException(Strings.IncorrectNumberOfBytesEncodedMessage);
                }
            }

            return (int) (stream.Position - start);
        }

        protected int WritePadding(Stream stream)
        {
            int bytesWritten = 0;

            foreach (DataBlock blk in m_padding) {
                bytesWritten += WriteDataBlock(stream, blk);
            }

            return bytesWritten;
        }

        /// <summary>
        /// Computes the size of a <see cref="DataBlock"/> when serialized.
        /// </summary>
        /// <param name="block">The block to get the size of.</param>
        /// <returns>The size of the data block in bytes.</returns>
        protected int GetBlockSize(DataBlock block)
        {
            int size = block.Data.Length;
            if (block.HasTag) {
                size += block.Tag.Length;
                if (block.StoreBlockSize) {
                    size += 4;
                }
            }

            return size;
        }

        protected int CountRemainingBlocks(Stream stream)
        {
            int count = 0;
            long mark = stream.Position;

            using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true)) {
                int blockSize = r.ReadInt32();
                while (r.BaseStream.Length - r.BaseStream.Position - 1 > blockSize) {
                    count++;
                    r.BaseStream.Position += blockSize;
                    blockSize = r.ReadInt32();
                }
            }

            stream.Position = mark;
            return count;
        }

        /// <summary>
        /// Creates a new <see cref="SaveDataFile"/> object from binary
        /// data found inside the specified file.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>The newly-created <see cref="SaveDataFile"/>.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown if the file is not a valid GTA3 save data file.
        /// </exception>
        public static SaveDataFile Load(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            GamePlatform fileType = DetectFileType(data);

            switch (fileType) {
                case GamePlatform.Android:
                    return Deserialize<SaveDataFileAndroid>(data);
                case GamePlatform.IOS:
                    return Deserialize<SaveDataFileIOS>(data);
                case GamePlatform.PC:
                    return Deserialize<SaveDataFilePC>(data);
                case GamePlatform.PlayStation2:
                    return Deserialize<SaveDataFilePS2>(data);
                case GamePlatform.Xbox:
                    return Deserialize<SaveDataFileXbox>(data);
                default:
                    throw new InvalidOperationException(
                        string.Format("{0} ({1})",
                            Strings.OopsMessage,
                            nameof(SaveDataFile)));
            }
        }

        /// <summary>
        /// Unpacks all data blocks into their respective data fields.
        /// </summary>
        protected void DeserializeDataBlocks()
        {
            CarGenerators = Deserialize<CarGeneratorsData>(m_carGenerators.Data);
        }

        /// <summary>
        /// Serializes all data fields and stores the result in the respective
        /// data blocks.
        /// </summary>
        protected void SerializeDataBlocks()
        {
            m_carGenerators.Data = Serialize(CarGenerators);
        }

        /// <summary>
        /// Adjusts an address so it becomes a multiple of 4 bytes (32 bits).
        /// Alignment occurs by rounding up.
        /// </summary>
        /// <param name="addr">The address to align.</param>
        /// <returns>The aligned address.</returns>
        protected static int Align32(int addr)
        {
            if (addr < 0) {
                return 0;
            }

            int retval = addr;
            if (addr % 4 != 0) {
                retval += 4 - addr % 4;
            }

            return retval;
        }

        /// <summary>
        /// Determines the file type of a GTA3 saved game. If the file
        /// type can't be determined, the file is declared invalid.
        /// </summary>
        private static GamePlatform DetectFileType(byte[] data)
        {
            const int UnknownConstant = 0x031401;

            using (BinaryReader r = new BinaryReader(new MemoryStream(data))) {
                bool isMobile;
                bool isPcOrXbox;
                bool isPs2;

                byte[] scrTag = Encoding.ASCII.GetBytes(ScriptsTag);
                int scrOffset = FindFirst(scrTag, data);
                if (scrOffset < 0) {
                    goto invalid_file;
                }

                isMobile = (scrOffset == 0xB8 && ReadInt(data, 0x34) == UnknownConstant);
                isPcOrXbox = (scrOffset == 0xC4 && ReadInt(data, 0x44) == UnknownConstant);
                isPs2 = (scrOffset == 0xB8 && ReadInt(data, 0x04) == UnknownConstant);

                int sizeOfBlock0 = ReadInt(data, 0x00);
                if (sizeOfBlock0 > data.Length) {
                    goto invalid_file;
                }

                int sizeOfBlock1 = ReadInt(data, sizeOfBlock0 + 0x04);

                if (isPs2) {
                    return GamePlatform.PlayStation2;
                }
                else if (isMobile) {
                    if (sizeOfBlock1 == 0x064C) {
                        return GamePlatform.Android;
                    }
                    else if (sizeOfBlock1 == 0x0648) {
                        return GamePlatform.IOS;
                    }
                }
                else if (isPcOrXbox) {
                    if (sizeOfBlock1 == 0x0624) {
                        return GamePlatform.PC;
                    }
                    else if (sizeOfBlock1 == 0x0628) {
                        return GamePlatform.Xbox;
                    }
                }

            invalid_file:
                throw new InvalidDataException(Strings.InvalidFileMessage);
            }
        }

        /// <summary>
        /// Locates the address of the first occurrence of a sequence in an array.
        /// </summary>
        private static int FindFirst(byte[] seq, byte[] arr)
        {
            int len = seq.Length;
            int limit = arr.Length - len;

            for (int i = 0; i <= limit; i++) {
                int k;
                for (k = 0; k < len; k++) {
                    if (seq[k] != arr[i + k]) {
                        break;
                    }
                }
                if (k == len) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Reads a 32-bit integer from an arbitrary address in an array.
        /// </summary>
        private static int ReadInt(byte[] data, int addr)
        {
            return BitConverter.ToInt32(data, addr);
        }
    }
}
