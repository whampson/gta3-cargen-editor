using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Properties;

namespace WHampson.Gta3CarGenEditor.Models
{
    public abstract class SaveDataFile : SerializableObject
    {
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
        protected DataBlock m_padding0;
        protected DataBlock m_padding1;

        protected SaveDataFile(GamePlatform fileType)
        {
            FileType = fileType;
            CarGeneratorsBlock = new CarGeneratorsDataBlock();

            m_simpleVars            = new DataBlock() { StoreBlockSize = false };
            m_scripts               = new DataBlock() { Tag = "SCR\0" };
            m_playerPeds            = new DataBlock();
            m_garages               = new DataBlock();
            m_vehicles              = new DataBlock();
            m_objects               = new DataBlock();
            m_pathFind              = new DataBlock();
            m_cranes                = new DataBlock();
            m_pickups               = new DataBlock();
            m_phoneInfo             = new DataBlock();
            m_restarts              = new DataBlock() { Tag = "RST\0" };
            m_radar                 = new DataBlock() { Tag = "RDR\0" };
            m_zones                 = new DataBlock() { Tag = "ZNS\0" };
            m_gangs                 = new DataBlock() { Tag = "GNG\0" };
            m_carGenerators         = new DataBlock() { Tag = "CGN\0" };
            m_particles             = new DataBlock();
            m_audioScriptObjects    = new DataBlock() { Tag = "AUD\0" };
            m_playerInfo            = new DataBlock();
            m_stats                 = new DataBlock();
            m_streaming             = new DataBlock();
            m_pedTypes              = new DataBlock() { Tag = "PTP\0" };
            m_padding0              = new DataBlock();
            m_padding1              = new DataBlock();
        }

        public GamePlatform FileType
        {
            get;
        }

        public CarGeneratorsDataBlock CarGeneratorsBlock
        {
            get;
            set;
        }

        public void Store(string path)
        {
            byte[] data = Serialize(this);
            File.WriteAllBytes(path, data);
        }

        protected int GetChecksum(Stream stream)
        {
            using (MemoryStream m = new MemoryStream()) {
                stream.Position = 0;
                stream.CopyTo(m);
                return m.ToArray().Sum(x => x);
            }
        }

        protected int ReadDataBlock(Stream stream, DataBlock block)
        {
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
                        throw new InvalidDataException();
                    }

                    // Read nested block size
                    if (block.StoreBlockSize) {
                        nestedBlockSize = r.ReadInt32();
                        if (nestedBlockSize != blockSize - tag.Length - 4) {
                            throw new InvalidDataException();
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
                    throw new InvalidDataException();
                }
            }

            return (int) (stream.Position - start);
        }

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
                    throw new InvalidDataException();
                }
            }

            return (int) (stream.Position - start);
        }

        protected int GetBlockSize(DataBlock block)
        {
            int size = block.Data.Length;
            //if (block.StoreBlockSize) {
            //    size += 4;
            //}
            if (block.HasTag) {
                size += block.Tag.Length;
                if (block.StoreBlockSize) {
                    size += 4;
                }
            }

            return size;
        }

        public static SaveDataFile Load(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            GamePlatform fileType = DetectFileType(data);

            switch (fileType) {
                case GamePlatform.PC:
                    return Deserialize<PCSaveDataFile>(data);
                case GamePlatform.PS2:
                    return Deserialize<PS2SaveDataFile>(data);
                default:
                    string plat = EnumHelper.GetAttribute<DescriptionAttribute>(fileType).Description;
                    string msg = string.Format(Resources.UnsupportedFileTypeMessage, plat);
                    throw new NotSupportedException(msg);
            }
        }

        private static GamePlatform DetectFileType(byte[] data)
        {
            const string ScriptsTag = "SCR\0";
            const int UnknownConstant = 0x031401;

            using (BinaryReader r = new BinaryReader(new MemoryStream(data))) {
                byte[] scrTag = Encoding.ASCII.GetBytes(ScriptsTag);

                bool isMobile;
                bool isPcOrXbox;
                bool isPs2;

                int scrOffset = FindFirst(scrTag, data);
                int sizeOfBlock1 = ReadInt(data, ReadInt(data, 0x00) + 0x04);

                isMobile = (scrOffset == 0xB8 && ReadInt(data, 0x34) == UnknownConstant);
                isPcOrXbox = (scrOffset == 0xC4 && ReadInt(data, 0x44) == UnknownConstant);
                isPs2 = (scrOffset == 0xB8 && ReadInt(data, 0x04) == UnknownConstant);

                if (isPs2) {
                    return GamePlatform.PS2;
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

                throw new InvalidDataException(Resources.InvalidFileMessage);
            }
        }

        private static int ReadInt(byte[] data, int addr)
        {
            return BitConverter.ToInt32(data, addr);
        }

        protected static int FindFirst(byte[] seq, byte[] arr)
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
    }
}
