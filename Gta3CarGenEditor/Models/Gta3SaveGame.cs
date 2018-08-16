using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using WHampson.Gta3CarGenEditor.Helpers;

namespace WHampson.Gta3CarGenEditor.Models
{
    public class Gta3SaveGame
    {
        private byte[] _block13;

        public Gta3SaveGame(string path)
        {
            Data = File.ReadAllBytes(path);
            SourcePath = path;
            FileType = DetectFileType(Data);

            ReadData();
        }

        public Gta3SaveGame(string path, GamePlatform fileType)
        {
            Data = File.ReadAllBytes(path);
            SourcePath = path;
            FileType = fileType;

            ReadData();
        }

        private byte[] Data
        {
            get;
        }

        public string Title
        {
            get;
            private set;
        }

        public GamePlatform FileType
        {
            get;
        }

        public string SourcePath
        {
            get;
        }

        public byte[] Block13_CarGenerators
        {
            get { return _block13; }
        }

        private void ReadData()
        {
            ReadTitle();

            switch (FileType) {
                case GamePlatform.PS2:
                    ReadBlocksPS2();
                    break;
                case GamePlatform.PC:
                    ReadBlocksPC();
                    break;
                default:
                    string plat = EnumHelper.GetAttribute<DescriptionAttribute>(FileType).Description;
                    throw new NotSupportedException("File type not yet supported: " + plat);
            }
        }

        private void ReadBlocksPS2()
        {
            BinaryReader r = new BinaryReader(new MemoryStream(Data));
            int blockIdx = -1;
            int blockSize;

            // Get to 3rd outer data block
            while (++blockIdx < 2) {
                blockSize = r.ReadInt32();
                r.BaseStream.Seek(blockSize, SeekOrigin.Current);
            }

            // Get to CarGenerators block
            blockSize = r.ReadInt32();
            blockIdx = -1;
            while (++blockIdx < 6) {
                blockSize = r.ReadInt32();
                r.BaseStream.Seek(blockSize, SeekOrigin.Current);
            }

            // Extract CarGenerators block
            blockSize = r.ReadInt32();
            _block13 = new byte[blockSize];
            r.Read(_block13, 0, blockSize);
        }

        private void ReadBlocksPC()
        {
            BinaryReader r = new BinaryReader(new MemoryStream(Data));
            int blockIdx = -1;
            int blockSize;

            // Get to CarGenerators block
            while (++blockIdx < 13) {
                blockSize = r.ReadInt32();
                r.BaseStream.Seek(blockSize, SeekOrigin.Current);
            }

            // Extract CarGenerators block
            blockSize = r.ReadInt32();
            blockSize = r.ReadInt32();
            _block13 = new byte[blockSize];
            r.Read(_block13, 0, blockSize);
        }

        private void ReadTitle()
        {
            BinaryReader r = new BinaryReader(new MemoryStream(Data));
            byte[] data;

            switch (FileType) {
                case GamePlatform.PS2:
                    Title = Path.GetFileName(SourcePath);
                    break;
                case GamePlatform.PC:
                    data = new byte[48];
                    r.BaseStream.Seek(4, SeekOrigin.Begin);
                    r.Read(data, 0, 48);
                    Title = Encoding.Unicode.GetString(data);
                    break;
                default:
                    Title = "(no title)";
                    break;
            }
        }

        public void Store()
        {
            Store(SourcePath);
        }

        public void Store(string path)
        {
            // TODO: other platforms
            //WriteDataPs2();
        }

        private static GamePlatform DetectFileType(byte[] data)
        {
            const int UnknownConstant = 0x031401;

            BinaryReader r = new BinaryReader(new MemoryStream(data));
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

            throw new InvalidDataException("Not a valid savegame.");
        }
        private static int ReadInt(byte[] data, int addr)
        {
            return BitConverter.ToInt32(data, addr);
        }

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
    }
}
