using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;

namespace WHampson.Gta3CarGenEditor.Models
{
    public abstract class SaveDataFile : SerializableObject
    {
        protected const int NumDataBlocks = 20;
        protected const int NumPaddingBlocks = 2;

        protected byte[] _block00;
        protected byte[] _block01;
        protected byte[] _block02;
        protected byte[] _block03;
        protected byte[] _block04;
        protected byte[] _block05;
        protected byte[] _block06;
        protected byte[] _block07;
        protected byte[] _block08;
        protected byte[] _block09;
        protected byte[] _block10;
        protected byte[] _block11;
        protected byte[] _block12;
        protected byte[] _block13;
        protected byte[] _block14;
        protected byte[] _block15;
        protected byte[] _block16;
        protected byte[] _block17;
        protected byte[] _block18;
        protected byte[] _block19;

        protected byte[] _pBlock00;
        protected byte[] _pBlock01;

        public SaveDataFile(GamePlatform fileType)
        {
            _block00 = new byte[0];
            _block01 = new byte[0];
            _block02 = new byte[0];
            _block03 = new byte[0];
            _block04 = new byte[0];
            _block05 = new byte[0];
            _block06 = new byte[0];
            _block07 = new byte[0];
            _block08 = new byte[0];
            _block09 = new byte[0];
            _block10 = new byte[0];
            _block11 = new byte[0];
            _block12 = new byte[0];
            _block13 = new byte[0];
            _block14 = new byte[0];
            _block15 = new byte[0];
            _block16 = new byte[0];
            _block17 = new byte[0];
            _block18 = new byte[0];
            _block19 = new byte[0];

            _pBlock00 = new byte[0];
            _pBlock01 = new byte[0];

            FileType = fileType;
            CarGeneratorsBlock = new CarGeneratorsDataBlock();
        }

        public GamePlatform FileType
        {
            get;
        }

        public CarGeneratorsDataBlock CarGeneratorsBlock
        {
            get;
            protected set;
        }

        public void Store(string path)
        {
            byte[] data = Serialize(this);
            File.WriteAllBytes(path, data);
        }

        protected List<byte[]> GetAllDataBlocks()
        {
            return new List<byte[]>()
            {
                _block00, _block01, _block02, _block03, _block04,
                _block05, _block06, _block07, _block08, _block09,
                _block10, _block11, _block12, _block13, _block14,
                _block15, _block16, _block17, _block18, _block19,
            };
        }

        protected List<byte[]> GetAllPaddingBlocks()
        {
            return new List<byte[]>()
            {
                _pBlock00, _pBlock01
            };
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
                    throw new NotSupportedException("File type not yet supported: " + plat);
            }
        }

        private static GamePlatform DetectFileType(byte[] data)
        {
            const int UnknownConstant = 0x031401;

            using (BinaryReader r = new BinaryReader(new MemoryStream(data))) {
                byte[] scrTag = Encoding.ASCII.GetBytes("SCR\0");

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

                throw new InvalidDataException("Not a valid GTA3 savegame.");
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
