using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;
using WHampson.Gta3CarGenEditor.Properties;

namespace WHampson.Gta3CarGenEditor.Models
{
    public abstract class SaveDataFile : SerializableObject
    {
        protected byte[] m_simpleVars;
        protected byte[] m_scripts;
        protected byte[] m_playerPeds;
        protected byte[] m_garages;
        protected byte[] m_vehicles;
        protected byte[] m_objects;
        protected byte[] m_pathFind;
        protected byte[] m_cranes;
        protected byte[] m_pickups;
        protected byte[] m_phoneInfo;
        protected byte[] m_restarts;
        protected byte[] m_radar;
        protected byte[] m_zones;
        protected byte[] m_gangs;
        protected CarGeneratorsDataBlock m_carGenerators;
        protected byte[] m_particles;
        protected byte[] m_audioScriptObjects;
        protected byte[] m_playerInfo;
        protected byte[] m_stats;
        protected byte[] m_streaming;
        protected byte[] m_pedTypes;
        protected byte[] m_padding0;
        protected byte[] m_padding1;

        protected SaveDataFile(GamePlatform fileType)
        {
            FileType = fileType;

            m_simpleVars = new byte[0];
            m_scripts = new byte[0];
            m_playerPeds = new byte[0];
            m_garages = new byte[0];
            m_vehicles = new byte[0];
            m_objects = new byte[0];
            m_pathFind = new byte[0];
            m_cranes = new byte[0];
            m_pickups = new byte[0];
            m_phoneInfo = new byte[0];
            m_restarts = new byte[0];
            m_radar = new byte[0];
            m_zones = new byte[0];
            m_gangs = new byte[0];
            m_carGenerators = new CarGeneratorsDataBlock();
            m_particles = new byte[0];
            m_audioScriptObjects = new byte[0];
            m_playerInfo = new byte[0];
            m_stats = new byte[0];
            m_streaming = new byte[0];
            m_pedTypes = new byte[0];
            m_padding0 = new byte[0];
            m_padding1 = new byte[0];
        }

        public GamePlatform FileType
        {
            get;
        }

        public CarGeneratorsDataBlock CarGeneratorsBlock
        {
            get { return m_carGenerators; }
            set { m_carGenerators = value; OnPropertyChanged(); }
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

        protected static int WordAlign(int addr)
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
